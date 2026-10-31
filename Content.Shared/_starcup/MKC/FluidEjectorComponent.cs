using Content.Shared.Damage;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._starcup.MKC
{
    [RegisterComponent, AutoGenerateComponentPause]
    public sealed partial class FluidEjectorComponent : Component
    {
        /// <summary>
        ///     The next time that the fluid ejector will attempt to begin the ejection process.
        /// </summary>
        [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
        public TimeSpan NextUpdate;

        /// <summary>
        ///     The amount of time it takes for fluids to be ejected once detected in the body.
        /// </summary>
        [DataField]
        public TimeSpan EjectionTime = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Damage applied to the entity when they eject fluids.
        /// </summary>
        [DataField(required: true)]
        public DamageSpecifier EjectionDamage;

        /// <summary>
        /// Multiplies EjectionDamage per unit of reagent expelled.
        /// </summary>
        [DataField(required: true)]
        public float EjectionDamageMultiplier;

        /// <summary>
        /// The amount of time between each intermittent popup during the fluid ejection process.
        /// </summary>
        [DataField]
        public TimeSpan PopupCooldown = TimeSpan.FromSeconds(6);

        /// <summary>
        /// The next time a popup will appear the fluid ejection process.
        /// </summary>
        [ViewVariables]
        public TimeSpan NextPopupTime;

        /// <summary>
        /// The solution this fluid ejector will check for updates on and eject from.
        /// </summary>
        [DataField]
        public string Solution = "bloodstream";
    }
}
