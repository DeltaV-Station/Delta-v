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
        public float DamageModifyFactor = default!;
        public float OffsetSwitching = 0.15f;
        public float ScaleFactor = 1f;
        public float DamageModifierCoefficient = default!;
        public float ExplosiveModifyFactor = default!;
        public EntityUid Lamia = default!;
        public int SegmentNumber = default!;
        public int MaxSegments = default!;
        public float DamageModifierConstant = default!;
        [DataField("segmentId")]
        public string? segmentId;
    }
}
