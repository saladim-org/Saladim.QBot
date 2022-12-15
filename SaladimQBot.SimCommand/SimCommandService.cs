using System.Drawing;
using System.Numerics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SaladimQBot.Core;
using SaladimQBot.SimCommand.Parsers;

namespace SaladimQBot.SimCommand;

public sealed class SimCommandService
{
    public static readonly Type StringType = typeof(string);

    private readonly List<MethodBasedCommand> commands;
    private readonly IServiceCollection? serviceCollection;

    public IClient Client { get; }

    public Dictionary<Type, Func<string, object>> CommandParamParsers { get; private set; }

    public string RootCommandPrefix { get; }

    /// <summary>
    /// 创建一个服务实例
    /// </summary>
    /// <param name="client">client实例</param>
    /// <param name="rootCommandPrefix">指令的前缀, 不需要前缀时请显式指定为空字符串</param>
    public SimCommandService(SimCommandServiceConfig config, IClient client, IServiceCollection? serviceCollection)
    {
        this.Client = client;
        this.RootCommandPrefix = config.RootPrefix;
        this.serviceCollection = serviceCollection;
        commands = new();
        CommandParamParsers = new()
        {
            [typeof(int)] = s => CommonTypeParsers.Int(s),
            [typeof(long)] = s => CommonTypeParsers.Long(s),
            [typeof(short)] = s => CommonTypeParsers.Short(s),
            [typeof(uint)] = s => CommonTypeParsers.Uint(s),
            [typeof(ulong)] = s => CommonTypeParsers.Ulong(s),
            [typeof(ushort)] = s => CommonTypeParsers.Ushort(s),
            [typeof(byte)] = s => CommonTypeParsers.Byte(s),
            [typeof(char)] = s => CommonTypeParsers.Char(s),
            [typeof(float)] = s => CommonTypeParsers.Float(s),
            [typeof(double)] = s => CommonTypeParsers.Double(s),
            [typeof(sbyte)] = s => CommonTypeParsers.Sbyte(s),
            [typeof(Vector2)] = s => CommonTypeParsers.Vector2(s),
            [typeof(Vector3)] = s => CommonTypeParsers.Vector3(s),
            [typeof(Color)] = s => CommonTypeParsers.Color(s),
        };
        foreach (var type in config.ModulesAssembly.DefinedTypes)
        {
            if (type.GetCustomAttribute<CommandModuleAttribute>() is not null)
            {
                this.AddModule(type);
            }
        }
    }

    private void AddModule(Type moduleClassType)
    {
        var cmdToAdd =
            from methodInfo in moduleClassType.GetMethods()
            let attr = methodInfo.GetCustomAttribute<CommandAttribute>()
            where attr is not null
            select new MethodBasedCommand(attr.Name, methodInfo);
        commands.AddRange(cmdToAdd);
        serviceCollection?.AddSingleton(moduleClassType);
    }

    /// <summary>
    /// 尝试匹配并且执行这个消息中的所有指令
    /// </summary>
    /// <param name="msg">消息</param>
    public void MatchAndExecuteAll(IMessage msg)
    {
        var allTextNodes = msg.MessageEntity.AllText();
        var matchedNodeTexts = allTextNodes
            .Where(node =>
            {
                var text = node.Text;
#if !NETSTANDARD2_0
                var trimedText = text.AsSpan().Trim();
                if (trimedText.StartsWith(RootCommandPrefix.AsSpan()))
                    return true;
#else
                var trimedText = text.Trim();
                if (trimedText.StartsWith(RootCommandPrefix))
                    return true;
#endif
                return false;
            })
            .Select(node => node.Text.Trim());
        foreach (var matchedNodeText in matchedNodeTexts)
        {
#if !NETSTANDARD2_0
            var cmdTextWithoutRootPrefix = matchedNodeText.AsSpan(RootCommandPrefix.Length);
#else
            var cmdTextWithoutRootPrefix = matchedNodeText.Substring(RootCommandPrefix.Length);
#endif
            foreach (var cmd in commands)
            {
#if !NETSTANDARD2_0
                if (cmdTextWithoutRootPrefix.StartsWith(cmd.Name.AsSpan()))
#else
                if (cmdTextWithoutRootPrefix.StartsWith(cmd.Name))
#endif
                {
                    //现在我们有一个文本以我们想要的前缀开头
                    //但是可能后面还有其他字符,我们再截取然后看看开头是不是空格
#if !NETSTANDARD2_0
                    var cmdTextWithoutPrefix = cmdTextWithoutRootPrefix.Slice(cmd.Name.Length);
#else
                    var cmdTextWithoutPrefix = cmdTextWithoutRootPrefix.Substring(cmd.Name.Length);
#endif
                    if (cmdTextWithoutPrefix.Length != 0)
                    {
                        if (cmdTextWithoutPrefix[0] == ' ')
                        {
                            //现在开头是空格了, 
                            //s/add 1 2, 此时cmdTextWithoutRootPrefix就是"add 1 2", 并且已经找到了想要的add指令
                            //add前缀去除, 得到形如" 1 2"的文本
                            //然后移除前导空格, 得到参数字符串
#if !NETSTANDARD2_0
                            var paramString = cmdTextWithoutPrefix.Slice(1);
#else
                            var paramString = cmdTextWithoutPrefix.Substring(1);
#endif
                            //此时形如"1 2"
                            //TODO 解析带引号的参数
                            var cmdParams = paramString.ToString().Split(' ');
                            if (cmdParams.Length != cmd.Parameters.Length)
                                continue;
                            ExecuteInternal(msg, cmd, cmdParams);
                        }
                    }
                    else
                        ExecuteInternal(msg, cmd, null);
                }
            }
        }
    }

    internal bool ExecuteInternal(IMessage msg, MethodBasedCommand cmd, string[]? cmdParams)
    {
        var paramsLength = cmd.Parameters.Length;
        if (Activator.CreateInstance(cmd.Method.DeclaringType!) is not CommandModule moduleIns) return false;
        moduleIns.Content = new(this, msg);
        if (moduleIns.PreCheck(moduleIns.Content))
        {
            var paramsTypes =
                from p in cmd.Parameters
                select p.ParameterType;
            if (cmdParams is null)
            {
                cmd.Method.Invoke(moduleIns, null);
            }
            else if (paramsTypes.All(t => t == StringType))
            {
                cmd.Method.Invoke(moduleIns, cmdParams);
            }
            else
            {
                object[] paramObjects = new object[paramsLength];
                for (int i = 0; i < paramsLength; i++)
                {
                    Type paramsType = paramsTypes.ElementAt(i);
                    if (paramsType == StringType)
                    {
                        paramObjects[i] = cmdParams[i];
                        continue;
                    }
                    if (!CommandParamParsers.TryGetValue(paramsType, out var parser))
                        throw new KeyNotFoundException($"Not found parser for type `{paramsType}`");
                    try
                    {
                        var parsedValue = parser(cmdParams[i]);
                        if (parsedValue is null)
                            return false;
                        paramObjects[i] = parsedValue;
                    }
                    catch
                    {
                        return false;
                    }
                }
                cmd.Method.Invoke(moduleIns, paramObjects);
            }
            return true;
        }
        return false;
    }

    public async Task MatchAndExecuteAllAsync(IMessage msg)
        => await Task.Run(() => MatchAndExecuteAll(msg)).ConfigureAwait(false);
}

internal class MethodBasedCommand
{
    public string Name;
    public MethodInfo Method;
    public ParameterInfo[] Parameters;

    public MethodBasedCommand(string name, MethodInfo method)
    {
        Name = name;
        Method = method;
        Parameters = method.GetParameters();
    }
}