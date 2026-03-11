using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PyrokinesisPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId => "ActionPyrokinesis";

    public override string PowerName => "psionic-power-name-Pyrokinesis";

    public override int MinGlimmerChanged => 10;

    public override int MaxGlimmerChanged => 30;

    /// <summary>
    /// How many firestacks will be added on the target.
    /// </summary>
    [DataField]
    public int AddedFirestacks = 5;
}
