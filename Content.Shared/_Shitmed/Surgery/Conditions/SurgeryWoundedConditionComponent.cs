using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SurgeryWoundedConditionComponent : Component
{
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<DamageGroupPrototype>)), AutoNetworkedField]
    public string DamageGroup = "Brute";
}
