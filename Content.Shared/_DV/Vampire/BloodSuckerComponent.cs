namespace Content.Shared._DV.Vampire
{
    [RegisterComponent]
    public partial class BloodSuckerComponent
    {
        /// <summary>
        /// How much to suck each time we suck.
        /// </summary>
        [DataField("unitsToSuck")]
        public float UnitsToSuck = 20f;

        /// <summary>
        /// The time (in seconds) that it takes to suck an entity.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan Delay = TimeSpan.FromSeconds(4);

        // ***INJECT WHEN SUCK***

        /// <summary>
        /// Whether to inject chems into a chemstream when we suck something.
        /// </summary>
        [DataField("injectWhenSuck")]
        public bool InjectWhenSuck;

        /// <summary>
        /// How many units of our injected chem to inject.
        /// </summary>
        [DataField("unitsToInject")]
        public float UnitsToInject = 5;

        /// <summary>
        /// Which reagent to inject.
        /// </summary>
        [DataField("injectReagent")]
        public string InjectReagent = "";

        /// <summary>
        /// Whether we need to web the thing up first...
        /// </summary>
        [DataField("webRequired")]
        public bool WebRequired;
    }
}
