using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PsionicInvisibilityPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId => "ActionPsionicInvisibility";

    public override string PowerName => "psionic-power-name-psionic-invisibility";

    public override int MinGlimmerChanged => 10;

    public override int MaxGlimmerChanged => 25;
}
