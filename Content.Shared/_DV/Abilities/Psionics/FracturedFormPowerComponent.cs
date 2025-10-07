using Content.Shared.Cloning;
using Robust.Shared.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Content.Shared.DoAfter;

namespace Content.Shared._DV.Abilities.Psionics;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FracturedFormPowerComponent : Component
{
    [DataField]
    public EntProtoId FracturedFormActionId = "ActionFracturedForm";

    [DataField, AutoNetworkedField]
    public EntityUid? FracturedFormActionEntity;
    [DataField]
    public DoAfterId? DoAfter;

    [DataField]
    public ProtoId<CloningSettingsPrototype> CopyNaked = "CloningPod";
    [DataField]
    public ProtoId<CloningSettingsPrototype> CopyClothed = "Antag";
    [DataField]
    public ProtoId<JobPrototype> VisitorJob = "Passenger";
    [DataField]
    public TimeSpan NextSwap = TimeSpan.MaxValue;
    [DataField]
    public float ManualSwapTime = 5f;
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
