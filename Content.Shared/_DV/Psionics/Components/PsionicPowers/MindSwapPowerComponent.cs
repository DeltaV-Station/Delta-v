using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MindSwapPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId => "ActionMindSwapPsionic";

    public override string PowerName => "psionic-power-name-mindswap";

    public override int MinGlimmerChanged => 5;

    public override int MaxGlimmerChanged => 30;
}
