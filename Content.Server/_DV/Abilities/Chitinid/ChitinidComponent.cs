using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Abilities.Chitinid;


[RegisterComponent]
public sealed partial class ChitinidComponent : Component
{
    [DataField("ChitzitePrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ChitzitePrototype = "Chitzite";

    [DataField("ChitziteActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? ChitziteActionId = "ActionChitzite";

    [DataField("ChitziteAction")]
    public EntityUid? ChitziteAction;

    [DataField]
    public float AmountAbsorbed { get; set; } = 0f;

    [DataField]
    public float MaximumAbsorbed = 30f;

    [DataField]
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate;
}
