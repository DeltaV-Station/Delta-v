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
        public int DamageModifyFactor = default!;
        public bool SexChanged = false;
        public EntityUid Lamia = default!;
        public int SegmentNumber = default!;
        public int MaxSegments = default!;
        [DataField("segmentId")]
        public string? segmentId;
    }
}
