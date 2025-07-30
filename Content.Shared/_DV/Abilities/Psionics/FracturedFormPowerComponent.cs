using Content.Shared.Cloning;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Roles;
using Robust.Shared.Audio;

namespace Content.Shared._DV.Abilities.Psionics;

[RegisterComponent]
public sealed partial class FracturedFormPowerComponent : Component
{
    public InstantActionComponent? MetapsionicPowerAction = null;
    [DataField("fracturedFormActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? FracturedFormActionId = "ActionFracturedForm";

    [DataField]
    public EntityUid? FracturedFormActionEntity;

    [DataField]
    public ProtoId<CloningSettingsPrototype> CopyNaked = "CloningPod";
    [DataField]
    public ProtoId<CloningSettingsPrototype> CopyClothed = "Antag";
    [DataField]
    public ProtoId<JobPrototype> VisitorJob = "Passenger";
    [DataField]
    public TimeSpan NextSwap = TimeSpan.MaxValue;
    [DataField]
    public bool SleepWarned = false;
    [DataField]
    public List<EntityUid> Bodies { get; set; } = new();
    [DataField]
    public SoundSpecifier SwapSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.05f)
    };
}
