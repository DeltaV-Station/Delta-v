using Content.Shared.Damage;
using Content.Shared.Tools;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._EstacaoPirata.BlindHealing
{
    [RegisterComponent]
    public sealed partial class BlindHealingComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)] [DataField("delay")]
        public int DoAfterDelay = 3;

        /// <summary>
        /// A multiplier that will be applied to the above if an entity is repairing themselves.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)] [DataField("selfHealPenalty")]
        public float SelfHealPenalty = 3f;

        /// <summary>
        /// Whether or not an entity is allowed to repair itself.
        /// </summary>
        [DataField("allowSelfHeal")]
        public bool AllowSelfHeal = true;

        [DataField("damageContainers", required: true)]
        public List<string> DamageContainers;
    }
}
