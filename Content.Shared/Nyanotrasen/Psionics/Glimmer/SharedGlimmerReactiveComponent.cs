using Robust.Shared.Audio;

namespace Content.Shared.Psionics.Glimmer
{
    [RegisterComponent]
    public sealed partial class SharedGlimmerReactiveComponent : Component
    {
        /// <summary>
        /// Do the effects of this component require power from an APC?
        /// </summary>
        [DataField("requiresApcPower")]
        public bool RequiresApcPower = false;

        /// <summary>
        /// Does this component try to modulate the strength of a PointLight
        /// component on the same entity based on the Glimmer tier?
        /// </summary>
        [DataField("modulatesPointLight")]
        public bool ModulatesPointLight = false;

        /// <summary>
        /// What is the correlation between the Glimmer tier and how strongly
        /// the light grows? The result is added to the base Energy.
        /// </summary>
        [DataField("glimmerToLightEnergyFactor")]
        public float GlimmerToLightEnergyFactor = 1.0f;

        /// <summary>
        /// What is the correlation between the Glimmer tier and how much
        /// distance the light covers? The result is added to the base Radius.
        /// </summary>
        [DataField("glimmerToLightRadiusFactor")]
        public float GlimmerToLightRadiusFactor = 1.0f;

        /// <summary>
        /// If this is true, this device has been locked by an event, is will not turn off until it is destroyed.
        /// </summary>
        [DataField]
        public bool Locked = false;

        /// <summary>
        /// If true, this component will scale its research generation based on the glimmer tier, as well as its glimmer generation.
        /// </summary>
        [DataField]
        public bool ScaleResearchGeneration = true;
        /// <summary>
        /// For each tier of glimmer, take the maximum possible glimmer value and multiply it by this factor to get the research generation factor.
        /// This is the research per second of this machine
        /// </summary>
        [DataField]
        public float ResearchGenerationFactor = 0.2f;
        /// <summary>
        /// For each tier of glimmer, take the maximum possible glimmer value and multiply it
        /// by this factor to get the glimmer generation per second.
        /// </summary>
        [DataField]
        public float GlimmerGenerationFactor = 0.001f;

        /// <summary>
        /// Noises to play on failed turn off.
        /// </summary>
        [DataField("shockNoises")]
        public SoundSpecifier ShockNoises { get; private set; } = new SoundCollectionSpecifier("sparks");
    }
}
