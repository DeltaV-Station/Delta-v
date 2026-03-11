using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MetapsionicPulsePowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId => "ActionMetapsionicPulse";

    public override string PowerName => "psionic-power-name-metapsionic";

    public override int MinGlimmerChanged => 1;

    public override int MaxGlimmerChanged => 10;

    /// <summary>
    /// The radius of the pulse.
    /// </summary>
    [DataField]
    public float Range = 1.5f;
}
