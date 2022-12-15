using System.Drawing;
using System.Numerics;
using Saladim.SalLogger;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp;
using SaladimQBot.SimCommand;

namespace SaladimSimCmd;

public class Program
{
    public static CqClient client = null!;
    public static SimCommandService simCmd = null!;

    public static async Task Main(string[] args)
    {
        client = new CqWebSocketClient("ws://127.0.0.1:5000", LogLevel.Trace);
        client.OnLog += Console.WriteLine;
        client.OnGroupMessageReceived += Client_OnGroupMessageReceived;
        simCmd = new(client, "");
        await client.StartAsync();
        Console.ReadLine();
        await client.StopAsync();
    }

    private static void Client_OnGroupMessageReceived(GroupMessage message, JoinedGroup group)
    {
        if (message.Group.GroupId == 860355679)
        {
            _ = simCmd.MatchAndExecuteAllAsync(message);
        }
    }
}

public class SampleModule : CommandModule
{
    [Command("random")]
    public void Random()
    {
        var builder = Content.Client.CreateMessageBuilder(Content.Message);
        builder.WithTextLine($"这是你的随机数~ {new Random().Next(0, 1000)}");
        _ = Content.MessageWindow.SendMessageAsync(builder.Build());
    }

    [Command("耗时方法")]
    public void TestMcd2()
    {
        Content.MessageWindow.SendMessageAsync("这个任务很耗时哦, 等会");
        Thread.Sleep(5000);
        Content.MessageWindow.SendMessageAsync("任务完成粒!");
    }

    [Command("echo")]
    public void Echo(string contentToEcho)
    {
        _ = Content.MessageWindow.SendMessageAsync(contentToEcho);
    }

    [Command("add")]
    public void Add(float a, int b)
    {
        _ = Content.MessageWindow.SendMessageAsync($"结果是: {a + b}");
    }

    [Command("get_color")]
    public void GetColor(Color color)
    {
        _ = Content.MessageWindow.SendMessageAsync($"你的颜色是 {color}");
    }

    [Command("get_vector")]
    public void GetVector2(Vector2 vector)
    {
        _ = Content.MessageWindow.SendMessageAsync($"你的二维向量是: {vector}");
    }

    [Command("get_vector")]
    public void GetVector3(Vector3 vector)
    {
        _ = Content.MessageWindow.SendMessageAsync($"你的三维向量是: {vector}");
    }
}