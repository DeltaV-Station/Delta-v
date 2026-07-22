using Content.Shared.Chat.Prototypes; // DeltaV - death emotes

namespace Content.Shared.Emoting;

public sealed class EmoteAttemptEvent(EntityUid uid, EmotePrototype? emote) : CancellableEntityEventArgs // DeltaV - death emotes
{
    public EntityUid Uid { get; } = uid;
    public EmotePrototype? Emote { get; } = emote; // DeltaV - death emotes
}
