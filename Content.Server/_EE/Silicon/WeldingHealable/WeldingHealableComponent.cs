using Content.Shared.Damage;
using Content.Shared.Tools;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._EE.Silicon.WeldingHealable
{
    [RegisterComponent]
    public sealed partial class WeldingHealableComponent : Component
    {
        /// <summary>
        /// DeltaV: Disables self-healing with any type of WeldingHealing item.
        /// </summary>
        [DataField]
        public bool AllowSelfHeal = true;
    }
}
