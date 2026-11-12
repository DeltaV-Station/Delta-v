using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TelekinesisPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionTelekinesis";

    public override string PowerName { get; set; } = "psionic-power-name-Telekinesis";

    public override int MinGlimmerChanged { get; set; } = 5;

    public override int MaxGlimmerChanged { get; set; } = 15;

    // Larger than it should ever need to be, just here for the occluded check.
    public float Range = 25f;
}
