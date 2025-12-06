using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TestPowerComponent : BasePsionicPowerComponent
{
     public override EntProtoId ActionProtoId => "ActionTestPower";

     /// <summary>
     /// The Loc string for the name of the power.
     /// </summary>
     public override string PowerName => "psionic-power-name-test";
}
