using Content.Shared.Speech;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Loudspeaker.Events;

[ByRefEvent]
public record struct GetLoudspeakerDataEvent(
    bool IsActive = false,
    int? FontSize = null,
    bool AffectRadio = false,
    bool AffectChat = false,
    ProtoId<SpeechSoundsPrototype>? SpeechSounds = null);
