using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Abilities.Psionics;

[RegisterComponent, NetworkedComponent]
public sealed partial class MindSwapPowerComponent : Component
{
    [DataField]
    public EntProtoId? MindSwapActionId = "ActionMindSwap";

    [DataField]
    public EntityUid? MindSwapActionEntity;
}
