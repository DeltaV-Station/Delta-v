using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared._DV.Damage.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DamageTypeTransferComponent : Component
{
    /// <summary>
    /// Transfers all damage of type `key` to instead be of type `value`
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<DamageTypePrototype>, ProtoId<DamageTypePrototype>> Transfers = new();
}
