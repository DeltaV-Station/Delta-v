using Content.Shared.Damage;
using Content.Shared.Tools;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._EstacaoPirata.WeldingHealing
{
    [RegisterComponent]
    public sealed partial class WeldingHealingComponent : Component
    {
        /// <summary>
        ///     All the damage to change information is stored in this <see cref="DamageSpecifier"/>.
        /// </summary>
        /// <remarks>
        ///     If this data-field is specified, it will change damage by this amount instead of setting all damage to 0.
        ///     in order to heal/repair the damage values have to be negative.
        /// </remarks>

        [ViewVariables(VVAccess.ReadWrite)] [DataField("damage", required: true)]
        public DamageSpecifier Damage;

        [ViewVariables(VVAccess.ReadWrite)] [DataField("qualityNeeded", customTypeSerializer:typeof(PrototypeIdSerializer<ToolQualityPrototype>))]
        public string QualityNeeded = "Welding";

        // The fuel amount needed to repair physical related damage
        [ViewVariables(VVAccess.ReadWrite)] [DataField("fuelCost")]
        public int FuelCost = 5;

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
