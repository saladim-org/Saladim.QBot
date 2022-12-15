using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CodingSeb.ExpressionEvaluator;
using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;
using SaladimQBot.Shared;
using System.Drawing;
using System.Net;
using SaladimQBot.SimCommand;
using SaladimWpf.Services;
using SaladimQBot.Core;

namespace SaladimWpf;

public class BotService
{
    protected CqClient cqClient;
    protected Logger logger;

    protected Random random;
    protected SimCommandService simCmdService;
    protected HttpService httpService;
    protected HomoService homoService;

    public event Action<string>? OnClientLog;

    public event Action<string>? OnLog;

    public CqClient Client { get => cqClient; }

    public bool OpenGuessNumberBot { get; set; }

    public int GuessNumberBotDelay { get; set; }

    public BotService(BotServiceConfig config, SimCommandService simCmdService, HttpService httpService, HomoService homoService)
    {
        cqClient = new CqWebSocketClient(config.Address, LogLevel.Trace);
        cqClient.OnLog += s => OnClientLog?.Invoke(s);
        logger = new LoggerBuilder()
            .WithLevelLimit(LogLevel.Trace)
            .WithAction(s => OnLog?.Invoke(s))
            .Build();

        cqClient.OnMessageReceived += Client_OnMessageReceived;

        var dateTimeNow = DateTime.Now;
        random = new(dateTimeNow.Millisecond + dateTimeNow.Second + dateTimeNow.Day + dateTimeNow.Minute);
        this.simCmdService = simCmdService;
        this.httpService = httpService;
        this.homoService = homoService;
    }

    private async void Client_OnMessageReceived(Message message)
    {
        await Task.Run(OnMessageReceived).ConfigureAwait(false);
        async void OnMessageReceived()
        {
            #region log
            if (message is GroupMessage groupMsg)
            {
                logger.LogInfo(
                    "WpfConsole", $"{groupMsg.Group.Name.Value}({groupMsg.Group.GroupId}) - " +
                    $"{groupMsg.Author.FullName} 说: " +
                    $"{groupMsg.MessageEntity.RawString}"
                    );
#if DEBUG
                if (groupMsg.Group.GroupId != 860355679)
                    return;
#endif
            }
            else if (message is PrivateMessage privateMsg)
            {
                logger.LogInfo(
                    "WpfConsole", $"{await privateMsg.Sender.Nickname.ValueAsync.ConfigureAwait(false)}" +
                    $"({privateMsg.Sender.UserId}) 私聊你: {privateMsg.MessageEntity.RawString}"
                    );
            }
            #endregion

            string rawString = message.MessageEntity.RawString.Trim();
            string commandStart = string.Empty;

            #region /random
            commandStart = "/random";
            if (rawString.StartsWith(commandStart))
            {

            }
            #endregion

            #region /算
            commandStart = "/算 ";
            if (rawString.StartsWith(commandStart))
            {
                try
                {
                    string thing = rawString[commandStart.Length..];
                    ExpressionEvaluator e = new();
                    string rst = e.Evaluate(thing)?.ToString() ?? "";
                    await message.MessageWindow.SendMessageAsync($"计算结果是: {rst}").ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    logger.LogWarn("Program", "Calculate", e);
                    string s = $"错误发生了, 以下是错误信息:\n{e.Message}";
                    await message.MessageWindow.SendMessageAsync(s).ConfigureAwait(false);
                }
            }
            #endregion

            #region /狠狠骂我
            commandStart = "/狠狠骂我";
            if (rawString.StartsWith(commandStart))
            {
                var msg = await message.MessageWindow.SendMessageAsync("cnm, 有病吧").ConfigureAwait(false);
                await Task.Delay(2000).ConfigureAwait(false);
                await msg.RecallAsync().ConfigureAwait(false);
                await Task.Delay(2000).ConfigureAwait(false);
                await message.MessageWindow.SendMessageAsync("qwq我刚才肯定没有骂人").ConfigureAwait(false);

            }
            #endregion

            #region /来点图
            commandStart = "/来点图";
            if (rawString.StartsWith(commandStart))
            {
                string s = await httpService.HttpClient.GetStringAsync("https://img.xjh.me/random_img.php?return=json").ConfigureAwait(false);
                string imgUrl = "http:" + JsonDocument.Parse(s).RootElement.GetProperty("img").GetString();
                MessageEntity m = new MessageEntityBuilder(cqClient)
                    .WithImage(imgUrl)
                    .Build();
                await message.MessageWindow.SendMessageAsync(m).ConfigureAwait(false);
            }
            #endregion

            #region /视频信息
            commandStart = "/视频信息 ";
            if (rawString.StartsWith(commandStart))
            {
                string query = "";
                string input = rawString[commandStart.Length..];
                if (input.StartsWith("BV"))
                    query = "bvid=" + input;
                else if (long.TryParse(input, out _))
                    query = "aid=" + input;
                else
                {
                    await message.MessageWindow.SendMessageAsync(
                        "输入格式不正确, 已取消本次api请求.\n" +
                        "可接受的格式: BV开头的bv号(如:BV17x411w7KC), 纯数字的av号(如:170001)"
                        ).ConfigureAwait(false);
                    return;
                }
                string url = $"https://api.bilibili.com/x/web-interface/view?{query}";
                var d =
                    JsonSerializer.Deserialize<Models.BilibiliVideoInfoApiCallResult.Root>
                        (await httpService.HttpClient.GetStringAsync(url).ConfigureAwait(false));
                if (d is null)
                    return;
                long code = d.Code;
                string? errMsg = d.Message;
                if (code == 0)
                {
                    StringBuilder sb = new();
                    sb.AppendLine("视频信息如下: ");
                    sb.AppendLine($"av号: av{d.Data.Aid}, bv号: {d.Data.Bvid}");
                    sb.AppendLine($"up主: {d.Data.Owner.Name}(uid{d.Data.Owner.Mid})");
                    sb.AppendLine($"视频标题: {d.Data.Title}");
                    sb.AppendLine($"分区: {d.Data.Tname}");
                    sb.AppendLine($"发布时间: {DateTimeHelper.GetFromUnix(d.Data.Pubdate)}");
                    sb.AppendLine($"播放: {d.Data.Stat.View:###,###}, 弹幕: {d.Data.Stat.Danmaku:###,###}");
                    sb.AppendLine($"视频封面地址: {d.Data.Pic}");
                    sb.AppendLine($"视频长度: {TimeSpan.FromSeconds(d.Data.Duration)}");
                    sb.AppendLine($"弹幕cid: {d.Data.Cid}");
                    if (d.Data.Videos != 1)
                        sb.AppendLine($"视频分p数: {d.Data.Videos}");
                    sb.Append($"点赞: {d.Data.Stat.Like:###,###}, 投币: {d.Data.Stat.Coin:###,###}, ");
                    sb.AppendLine($"收藏: {d.Data.Stat.Favorite:###,###}, 分享: {d.Data.Stat.Share}");
                    sb.AppendLine($"版权信息: {(d.Data.Copyright == 1 ? "原创" : "转载")}");
                    sb.AppendLine($"动态信息: {d.Data.Dynamic}");
                    sb.AppendLine($"视频简介: {d.Data.Desc.Replace("\n", " {n} ")}");
                    await message.MessageWindow.SendMessageAsync(sb.ToString()).ConfigureAwait(false);
                }
                else
                {
                    await message.MessageWindow.SendMessageAsync($"错误发生了, 错误代码:{code}, 错误信息:\n{errMsg}").ConfigureAwait(false);
                }
            }
            #endregion

            #region /来点颜色

            commandStart = "/来点颜色";
            if (rawString.StartsWith(commandStart))
            {
                Debug.Assert(OperatingSystem.IsWindows());
                var bitmap = new Bitmap(50, 50);
                var graphics = Graphics.FromImage(bitmap);
                var color = GetRandomColor();
                graphics.DrawRectangle(new Pen(color, 256), new Rectangle(0, 0, 256, 256));
                if (!Directory.Exists("tempImages")) Directory.CreateDirectory("tempImages");
                var imgName = $"{DateTime.Now.Ticks - 638064687298838726L}.png";
                var fileName = $"tempImages\\{imgName}";
                bitmap.Save(fileName);
                MessageEntity entity = new MessageEntityBuilder(cqClient)
                    .WithImage("http://127.0.0.1:5702/?img_name=" + imgName)
                    .WithTextLine($"\nRGB: {color.R},{color.G},{color.B}")
                    .WithText($"HEX: #{color.R:X}{color.G:X}{color.B:X}")
                    .Build();
                await message.MessageWindow.SendMessageAsync(entity).ConfigureAwait(false);
            }

            #endregion

            #region /choose

            commandStart = "/choose ";
            do
            {
                if (rawString.StartsWith(commandStart))
                {
                    var paramString = rawString[commandStart.Length..];
                    var splitedString = paramString.Split(" ");
                    var count = splitedString.Length;
                    if (count <= 1)
                        break;
                    var result = random.Next(count);
                    await message.MessageWindow.SendMessageAsync($"选择的最终结果是...\n『{splitedString[result]}』");
                }
            } while (false);

            #endregion

            #region

            commandStart = "/homo ";
            if (rawString.StartsWith(commandStart))
            {
                var numStr = rawString[commandStart.Length..];
                if (double.TryParse(numStr, out var d))
                {
                    string rst = homoService.Homo(d);
                    if (rst.Length <= 150)
                    {
                        await message.MessageWindow.SendMessageAsync($"{d} = {rst}");
                    }
                    else
                    {
                        MessageEntity b2 = new MessageEntityBuilder(cqClient)
                            .WithText("消息长度过长, 以自动转为转发消息.")
                            .Build();
                        MessageEntity b = new MessageEntityBuilder(cqClient)
                            .WithText($"{d} = {rst}")
                            .Build();
                        ForwardEntityBuilder f = new(cqClient);
                        f.AddMessage(cqClient.Self, b, DateTime.Now);
                        f.AddMessage(cqClient.Self, b2, DateTime.Now);
                        if (message is GroupMessage gmsg)
                        {
                            await gmsg.Group.SendMessageAsync(f.Build());
                        }
                    }
                }
            }

            #endregion

            #region auto猜数游戏
            //auto猜数游戏
            if (OpenGuessNumberBot && rawString.Contains("您猜了") && rawString.Contains("但是猜的数"))
            {
                const string currentRegion = "当前范围:";
                int loc = rawString.IndexOf(currentRegion);
                if (loc is not -1)
                {
                    string regionString = rawString[(loc + currentRegion.Length)..];
                    string[] regions = regionString.Split("~");
                    long num1 = long.Parse(regions[0]);
                    long num2 = long.Parse(regions[1]);
                    long target = (num1 + num2) / 2;
                    logger.LogInfo("WpfConsole", "猜数", $"区域字符串为: {regionString}, 计算结果: {target}");
                    await Task.Delay(this.GuessNumberBotDelay).ConfigureAwait(false);
                    await message.MessageWindow.SendMessageAsync($"猜{target}").ConfigureAwait(false);
                }
            }
            #endregion
        }
    }

    private static Color GetRandomColor()
    {
        Random r = new((int)DateTime.Now.Ticks);
        return Color.FromArgb(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));
    }

    public async Task StartAsync()
    {
        await cqClient.StartAsync().ConfigureAwait(false);
    }

    public async Task StopAsync()
    {
        await cqClient.StopAsync().ConfigureAwait(false);
    }
}
