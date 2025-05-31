using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Curation;

[Serializable, NetSerializable]
public sealed class CwoinkTextMessage : EntityEventArgs
{
    public DateTime SentAt { get; }

    public NetUserId UserId { get; }

    // This is ignored from the client.
    // It's checked by the client when receiving a message from the server for cwoink noises.
    // This could be a boolean "Incoming", but that would require making a second instance.
    public NetUserId TrueSender { get; }
    public string Text { get; }

    public bool PlaySound { get; }

    public readonly bool AdminOnly;

    public CwoinkTextMessage(NetUserId userId, NetUserId trueSender, string text, DateTime? sentAt = default, bool playSound = true, bool adminOnly = false)
    {
        SentAt = sentAt ?? DateTime.Now;
        UserId = userId;
        TrueSender = trueSender;
        Text = text;
        PlaySound = playSound;
        AdminOnly = adminOnly;
    }
}
