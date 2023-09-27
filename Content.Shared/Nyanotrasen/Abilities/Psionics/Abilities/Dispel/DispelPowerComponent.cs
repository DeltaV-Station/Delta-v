using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class DispelPowerComponent : Component
    {
        [DataField("range")]
        public float Range = 10f;

        public EntityTargetActionComponent? DispelPowerAction = null;

        [DataField("dispelAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string DispelAction = "ActionDispel";

        [ValidatePrototypeId<EntityPrototype>]
        public const string DispelActionPrototype = "ActionDispel";
    }
}
