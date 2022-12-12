namespace SaladimQBot.Core;

public interface IMessage : IClientEntity
{
    int MessageId { get; }

    IMessageEntity MessageEntity { get; }

    IUser Sender { get; }

    DateTime SendTime { get; }
}