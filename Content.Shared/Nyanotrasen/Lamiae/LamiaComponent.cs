namespace Content.Shared.Nyanotrasen.Lamiae
{
    /// <summary>
    /// Controls initialization of the multisegmented lamia species.
    /// </summary>
    [RegisterComponent]
    public sealed partial class LamiaComponent : Component
    {
        /// <summary>
        /// A list of each UID attached to the Lamia, in order of spawn
        /// </summary>
        public List<EntityUid> Segments = new();

        [DataField("numberOfSegments")]
        public int NumberOfSegments = 18;
    }
}
