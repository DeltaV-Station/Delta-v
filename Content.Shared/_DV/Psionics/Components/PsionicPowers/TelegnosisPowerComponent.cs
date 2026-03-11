using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedTelegnosisPowerSystem))]
public sealed partial class TelegnosisPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId => "ActionTelegnosis";

    public override string PowerName => "psionic-power-name-telegnosis";

    public override int MinGlimmerChanged => 10;

    public override int MaxGlimmerChanged => 50;

    /// <summary>
    /// The mob prototype that will be used to telegnosis.
    /// </summary>
    [DataField]
    public EntProtoId Prototype = "MobObserverTelegnostic";
}

