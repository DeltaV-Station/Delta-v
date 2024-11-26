using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NanoChatUiMessageEvent : CartridgeMessageEvent
{
    /// <summary>
    ///     The type of UI message being sent.
    /// </summary>
    public readonly NanoChatUiMessageType Type;

    /// <summary>
    ///     The recipient's NanoChat number, if applicable.
    /// </summary>
    public readonly uint? RecipientNumber;

    /// <summary>
    ///     The content of the message or name for new chats.
    /// </summary>
    public readonly string? Content;

    /// <summary>
    ///     The recipient's job title when creating a new chat.
    /// </summary>
    public readonly string? RecipientJob;

    /// <summary>
    ///     Creates a new NanoChat UI message event.
    /// </summary>
    /// <param name="type">The type of message being sent</param>
    /// <param name="recipientNumber">Optional recipient number for the message</param>
    /// <param name="content">Optional content of the message</param>
    /// <param name="recipientJob">Optional job title for new chat creation</param>
    public NanoChatUiMessageEvent(NanoChatUiMessageType type,
        uint? recipientNumber = null,
        string? content = null,
        string? recipientJob = null)
    {
        Type = type;
        RecipientNumber = recipientNumber;
        Content = content;
        RecipientJob = recipientJob;
    }
}

[Serializable, NetSerializable]
public enum NanoChatUiMessageType : byte
{
    NewChat,
    SelectChat,
    CloseChat,
    SendMessage,
    DeleteChat,
    ToggleMute,
}

// putting this here because i can
[Serializable, NetSerializable, DataRecord]
public struct NanoChatRecipient
{
    /// <summary>
    ///     The recipient's unique NanoChat number.
    /// </summary>
    public uint Number { get; set; }

    /// <summary>
    ///     The recipient's display name, typically from their ID card.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     The recipient's job title, if available.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    ///     Whether this recipient has unread messages.
    /// </summary>
    public bool HasUnread { get; set; }

    /// <summary>
    ///     Creates a new NanoChat recipient.
    /// </summary>
    /// <param name="number">The recipient's NanoChat number</param>
    /// <param name="name">The recipient's display name</param>
    /// <param name="jobTitle">Optional job title for the recipient</param>
    /// <param name="hasUnread">Whether there are unread messages from this recipient</param>
    public NanoChatRecipient(uint number, string name, string? jobTitle = null, bool hasUnread = false)
    {
        Number = number;
        Name = name;
        JobTitle = jobTitle;
        HasUnread = hasUnread;
    }
}

[Serializable, NetSerializable, DataRecord]
public struct NanoChatMessage
{
    /// <summary>
    ///     When the message was sent.
    /// </summary>
    public TimeSpan Timestamp { get; set; }

    /// <summary>
    ///     The content of the message.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    ///     The NanoChat number of the sender.
    /// </summary>
    public uint SenderId { get; set; }

    /// <summary>
    ///     Whether the message failed to deliver to the recipient.
    ///     This can happen if the recipient is out of range or if there's no active telecomms server.
    /// </summary>
    public bool DeliveryFailed { get; set; }

    /// <summary>
    ///     Creates a new NanoChat message.
    /// </summary>
    /// <param name="timestamp">When the message was sent</param>
    /// <param name="content">The content of the message</param>
    /// <param name="senderId">The sender's NanoChat number</param>
    /// <param name="deliveryFailed">Whether delivery to the recipient failed</param>
    public NanoChatMessage(TimeSpan timestamp, string content, uint senderId, bool deliveryFailed = false)
    {
        Timestamp = timestamp;
        Content = content;
        SenderId = senderId;
        DeliveryFailed = deliveryFailed;
    }
}
