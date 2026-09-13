using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HystericalStrengthPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionHystericalStrength";

    public override string PowerName { get; set; } = "psionic-power-name-hysterical-strength";

    public override int MinGlimmerChanged { get; set; } = 0;

    public override int MaxGlimmerChanged { get; set; } = 0;

    /// <summary>
    /// The density added to the user when the power is active.
    /// </summary>
    [DataField]
    public float AddedDensity = 500;

    /// <summary>
    /// The prototype ID of the status effect.
    /// </summary>
    [DataField]
    public EntProtoId HystericalStrengthEffectProto = "HystericalStrengthStatusEffect";
}
