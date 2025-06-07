using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Curation;

[Serializable, NetSerializable]
public sealed class CwoinkTextMessage(
    NetUserId userId,
    NetUserId trueSender,
    string text,
    DateTime? sentAt = default,
    bool playSound = true,
    bool adminOnly = false)
    : EntityEventArgs
{
    public DateTime SentAt { get; } = sentAt ?? DateTime.Now;

    public NetUserId UserId { get; } = userId;

    // This is ignored from the client.
    // It's checked by the client when receiving a message from the server for cwoink noises.
    // This could be a boolean "Incoming", but that would require making a second instance.
    public NetUserId TrueSender { get; } = trueSender;
    public string Text { get; } = text;

    public bool PlaySound { get; } = playSound;

    public readonly bool AdminOnly = adminOnly;
}
