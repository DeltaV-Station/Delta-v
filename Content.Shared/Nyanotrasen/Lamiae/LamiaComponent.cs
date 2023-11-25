namespace Content.Shared.Nyanotrasen.Lamiae
{
    /// <summary>
    /// Controls initialization of the multisegmented lamia species.
    /// </summary>
    [RegisterComponent]
    public sealed partial class LamiaComponent : Component
    {
        public List<EntityUid> Segments = new();

        [DataField("numberOfSegments")]
        public int NumberOfSegments = 32;
    }
}
