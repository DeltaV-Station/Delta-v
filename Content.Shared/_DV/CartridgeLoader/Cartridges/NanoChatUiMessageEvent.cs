using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.CartridgeLoader.Cartridges;

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
    EditChat,
    CloseChat,
    SendMessage,
    DeleteChat,
    ToggleMute,
    ToggleMuteChat,
    ToggleListNumber,
    CreateGroupChat, // Funky Station - Group Chats
    InviteToGroup, // Funky Station - Group Chats
    KickFromGroup, // Funky Station - Group Chats
    ViewGroupMembers, // Funky Station - Group Chats
    AdminUser, // Funky Station - Group Chats
    DeadminUser, // Funky Station - Group Chats
}

// putting this here because i can
[Serializable, NetSerializable, DataRecord]
public struct NanoChatRecipient
{
    /// <summary>
    ///     The recipient's unique NanoChat number.
    /// </summary>
    public uint Number;

    /// <summary>
    ///     The recipient's display name, typically from their ID card.
    /// </summary>
    public string Name;

    /// <summary>
    ///     The recipient's job title, if available.
    /// </summary>
    public string? JobTitle;

    /// <summary>
    ///     Whether this recipient has unread messages.
    /// </summary>
    public bool HasUnread;

    /// <summary>
    ///     Whether this is a group chat.
    /// </summary>
    public bool IsGroup; // Funky Station - Group Chats

    /// <summary>
    ///     For group chats: list of member NanoChat numbers.
    /// </summary>
    public HashSet<uint>? Members; // Funky Station - Group Chats

    /// <summary>
    ///     For group chats: the NanoChat number of the creator.
    /// </summary>
    public uint? CreatorId; // Funky Station - Group Chats

    /// <summary>
    ///     For group chats: set of admin NanoChat numbers who can invite/kick.
    /// </summary>
    public HashSet<uint>? Admins; // Funky Station - Group Chats

    /// <summary>
    ///     Creates a new NanoChat recipient.
    /// </summary>
    /// <param name="number">The recipient's NanoChat number</param>
    /// <param name="name">The recipient's display name</param>
    /// <param name="jobTitle">Optional job title for the recipient</param>
    /// <param name="hasUnread">Whether there are unread messages from this recipient</param>
    /// <param name="isGroup">Whether this is a group chat</param>
    /// <param name="members">For group chats: list of member NanoChat numbers</param>
    /// <param name="creatorId">For group chats: the creator's NanoChat number</param>
    /// <param name="admins">For group chats: set of admin NanoChat numbers</param>
    public NanoChatRecipient(uint number,
        string name,
        string? jobTitle = null,
        bool hasUnread = false,
        bool isGroup = false, // Funky Station - Group Chats
        HashSet<uint>? members = null, // Funky Station - Group Chats
        uint? creatorId = null, // Funky Station - Group Chats
        HashSet<uint>? admins = null) // Funky Station - Group Chats
    {
        Number = number;
        Name = name;
        JobTitle = jobTitle;
        HasUnread = hasUnread;
        IsGroup = isGroup; // Funky Station - Group Chats
        Members = members; // Funky Station - Group Chats
        CreatorId = creatorId; // Funky Station - Group Chats
        Admins = admins; // Funky Station - Group Chats
    }
}

[Serializable, NetSerializable, DataRecord]
public struct NanoChatMessage
{
    public const int MaxContentLength = 256;

    /// <summary>
    ///     When the message was sent.
    /// </summary>
    public TimeSpan Timestamp;

    /// <summary>
    ///     The content of the message.
    /// </summary>
    public string Content;

    /// <summary>
    ///     The NanoChat number of the sender.
    /// </summary>
    public uint SenderId;

    /// <summary>
    ///     Whether the message failed to deliver to the recipient.
    ///     This can happen if the recipient is out of range or if there's no active telecomms server.
    /// </summary>
    public bool DeliveryFailed;

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

/// <summary>
///     NanoChat log data struct
/// </summary>
/// <remarks>Used by the LogProbe</remarks>
[Serializable, NetSerializable, DataRecord]
public readonly struct NanoChatData(
    Dictionary<uint, NanoChatRecipient> recipients,
    Dictionary<uint, List<NanoChatMessage>> messages,
    uint? cardNumber,
    NetEntity card)
{
    public Dictionary<uint, NanoChatRecipient> Recipients { get; } = recipients;
    public Dictionary<uint, List<NanoChatMessage>> Messages { get; } = messages;
    public uint? CardNumber { get; } = cardNumber;
    public NetEntity Card { get; } = card;
}

/// <summary>
///     Raised on the NanoChat card whenever a recipient gets added
/// </summary>
[ByRefEvent]
public readonly record struct NanoChatRecipientUpdatedEvent(EntityUid CardUid);

/// <summary>
///     Raised on the NanoChat card whenever it receives or tries sending a messsage
/// </summary>
[ByRefEvent]
public readonly record struct NanoChatMessageReceivedEvent(EntityUid CardUid);
