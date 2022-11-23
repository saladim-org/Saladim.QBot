﻿using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class CqMessageImageSendNode : CqMessageEntityNode, IMessageImageSendNode
{
    public override MessageNodeType NodeType { get => MessageNodeType.Image; }

    public string File { get; set; } = default!;

    public CqMessageImageSendNode(string file)
    {
        this.File = file;
    }
}