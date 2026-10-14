using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared._DV.Damage.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DamageTypeTransferComponent : Component
{
    [DataField("transfers", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<string, DamageTypePrototype>))]
    public Dictionary<string, string> Transfers = new();
}
