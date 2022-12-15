using System;
using SaladimQBot.Core;
using SaladimQBot.SimCommand;

namespace SaladimWpf.Modules;

[CommandModule]
public class TextMisc : CommandModule
{
    [Command("random")]
    public void Random()
    {
        Random r = new();
        int num = r.Next(0, 100);
        var msg = Content.Client.CreateMessageBuilder(Content.Message)
            .WithText($"{Content.Executer.Nickname},你的随机数为{num}哦~");
        _ = Content.MessageWindow.SendMessageAsync(msg.Build()).ConfigureAwait(false);
    }
}
