using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MetapsionicPulsePowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionMetapsionicPulse";

    public override string PowerName { get; set; } = "psionic-power-name-metapsionic";

    public override int MinGlimmerChanged { get; set; } = 1;

    public override int MaxGlimmerChanged { get; set; } = 10;

    /// <summary>
    /// The radius of the pulse.
    /// </summary>
    [DataField]
    public float Range = 1.5f;
}
