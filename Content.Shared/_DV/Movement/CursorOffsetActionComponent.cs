using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._DV.Movement;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CursorOffsetActionComponent : Component
{
    [DataField("cursorOffsetActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>),
        required: true)]
    public string? CursorOffsetActionId;

    [DataField("cursorOffsetActionEntity")]
    public EntityUid? CursorOffsetActionEntity;

    [DataField, AutoNetworkedField]
    public bool Active = false;
}
