using Robust.Shared.GameStates;

namespace Content.Shared.Nyanotrasen.Lamiae
{
    /// <summary>
    /// Lamia segment
    /// </summary>
    [RegisterComponent]
    [NetworkedComponent]
    public sealed partial class LamiaSegmentComponent : Component
    {
        public EntityUid AttachedToUid = default!;

        public bool SexChanged = false;
        public EntityUid Lamia = default!;
        public int SegmentNumber = default!;
        [DataField("segmentId")]
        public string? segmentId;
    }
}
