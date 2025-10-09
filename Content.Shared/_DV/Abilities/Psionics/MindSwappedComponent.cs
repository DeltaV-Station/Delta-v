using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Abilities.Psionics;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MindSwappedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid OriginalEntity = default!;
    [DataField]
    public EntProtoId? MindSwapReturnActionId = "ActionMindSwapReturn";

    [DataField]
    public EntityUid? MindSwapReturnActionEntity;
}
