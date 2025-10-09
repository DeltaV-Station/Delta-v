namespace Content.Server.Psionics
{
    [RegisterComponent]
    public sealed partial class PotentialPsionicComponent : Component
    {
        [DataField("chance")]
        public float Chance = 0.04f;

        /// <summary>
        /// YORO (you only reroll once)
        /// </summary>
        [DataField] // DeltaV - add to Variables UI for debugging assistance
        public bool Rerolled = false;
    }
}
