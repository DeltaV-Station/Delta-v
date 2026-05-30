using Content.Shared.Radio;

namespace Content.Shared._DV.Chat;

[ByRefEvent]
public record struct EntityAudiblyEmotedEvent(EntityUid Source, string Message, RadioChannelPrototype? Channel, EmoteType? Type);
