using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DispelPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId => "ActionDispel";

    public override string PowerName => "psionic-power-name-dispel";

    public override int MinGlimmerChanged => 5;

    public override int MaxGlimmerChanged => 10;
}
