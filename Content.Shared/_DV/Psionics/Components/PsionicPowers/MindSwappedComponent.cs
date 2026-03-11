using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MindSwappedReturnPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId => "ActionMindSwapReturn";

    public override string PowerName => "psionic-power-name-mindswap-return";

    public override int MinGlimmerChanged => 0;

    public override int MaxGlimmerChanged => 0;

    [DataField, AutoNetworkedField]
    public EntityUid OriginalEntity;
}
