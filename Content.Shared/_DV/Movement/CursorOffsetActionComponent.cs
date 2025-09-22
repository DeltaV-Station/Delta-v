using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._DV.Movement;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CursorOffsetActionComponent : Component
{
    [DataField]
    public EntProtoId CursorOffsetActionId = "ActionAvaliZoom";

    [DataField]
    public EntityUid? CursorOffsetActionEntity;

    [DataField, AutoNetworkedField]
    public bool Active = false;
}
