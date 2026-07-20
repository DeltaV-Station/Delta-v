namespace Content.Shared._Goobstation.Loudspeaker.Events;

[ByRefEvent]
public record struct GetLoudspeakerEvent(
    List<EntityUid> Loudspeakers);
