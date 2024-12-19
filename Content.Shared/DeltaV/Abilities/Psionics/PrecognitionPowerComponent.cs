using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Abilities.Psionics;

[RegisterComponent]
public sealed partial class PrecognitionPowerComponent : Component
{
    [DataField]
    public float RandomResultChance = 0.2F;

    [DataField]
    public Dictionary<EntityPrototype, PrecognitionResultComponent> AllResults;

    [DataField]
    public SoundSpecifier VisionSound = new SoundPathSpecifier("/Audio/DeltaV/Effects/clang2.ogg");

    [DataField]
    public EntityUid? SoundStream;

    [DataField]
    public DoAfterId? DoAfter;

    [DataField]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(8.35); // The length of the sound effect

    [DataField]
    public EntProtoId PrecognitionActionId = "ActionPrecognition";

    [DataField]
    public EntityUid? PrecognitionActionEntity;
}
