using Content.Shared.Chemistry.Reagent;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Footprints.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DecalScrubberComponent : Component
{
    [DataField]
    public float Radius = 1f;

    [DataField]
    public TimeSpan DoAfterLength = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public EntityCoordinates? LastClick;

    [DataField]
    public SoundSpecifier ScrubSound = AbsorbentComponent.DefaultTransferSound;

    [DataField]
    public string? CleaningSolutionName;

    [DataField]
    public ProtoId<ReagentPrototype> CleaningReagent = "Water";

    [DataField]
    public FixedPoint2 CleaningReagentCost = 5.0;
}

[Serializable, NetSerializable]
public sealed partial class DecalScrubberDoAfterEvent : SimpleDoAfterEvent;
