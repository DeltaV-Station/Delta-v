using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedTelegnosisPowerSystem))]
public sealed partial class TelegnosisPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionTelegnosis";

    public override string PowerName { get; set; } = "psionic-power-name-telegnosis";

    public override int MinGlimmerChanged { get; set; } = 10;

    public override int MaxGlimmerChanged { get; set; } = 50;

    /// <summary>
    /// The mob prototype that will be used to telegnosis.
    /// </summary>
    [DataField]
    public EntProtoId Prototype = "MobObserverTelegnostic";
}
