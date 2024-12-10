using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.Psionics;

[RegisterComponent]
public sealed partial class PrecognitionPowerComponent : Component
{
    [DataField("randomResultChance")]
    public float RandomResultChance = 0.2F;

    [DataField]
    public SoundSpecifier VisionSound = new SoundPathSpecifier("/Audio/DeltaV/Effects/clang2.ogg");

    [DataField]
    public EntityUid? SoundStream;

    [DataField("doAfter")]
    public DoAfterId? DoAfter;

    [DataField("useDelay")]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(8.35);

    [DataField("PrecognitionActionId",
    customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? PrecognitionActionId = "ActionPrecognition";

    [DataField("PrecognitionActionEntity")]
    public EntityUid? PrecognitionActionEntity;
}
