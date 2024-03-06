/*
* Delta-V - This file is licensed under AGPLv3
* Copyright (c) 2024 Delta-V Contributors
* See AGPLv3.txt for details.
*/

namespace Content.Shared.DeltaV.Lamiae
{
    /// <summary>
    /// Controls initialization of any Multi-segmented entity
    /// </summary>
    [RegisterComponent]
    public sealed partial class LamiaComponent : Component
    {
        /// <summary>
        /// A list of each UID attached to the Lamia, in order of spawn
        /// </summary>
        public List<EntityUid> Segments = new();

        /// <summary>
        /// A clamped variable that represents the number of segments to be spawned
        /// </summary>
        [DataField("numberOfSegments")]
        public int NumberOfSegments = 18;

        /// <summary>
        /// If UseTaperSystem is true, this constant represents the rate at which a segmented entity will taper towards the tip. Tapering is on a logarithmic scale, and will asymptotically approach 0.
        /// </summary>
        [DataField("offsetConstant")]
        public float OffsetConstant = 1.03f;

        /// <summary>
        /// Represents the prototype used to parent all segments
        /// </summary>
        [DataField("initialSegmentId")]
        public string InitialSegmentId = "LamiaInitialSegment";

        /// <summary>
        /// Represents the segment prototype to be spawned
        /// </summary>
        [DataField("SegmentId")]
        public string SegmentId = "LamiaSegment";

        /// <summary>
        /// Toggles the tapering system on and off. When false, segmented entities will have a constant width.
        /// </summary>
        [DataField("useTaperSystem")]
        public bool UseTaperSystem = true;

        /// <summary>
        /// The standard distance between the centerpoint of each segment.
        /// </summary>
        [DataField("staticOffset")]
        public float StaticOffset = 0.15f;

        /// <summary>
        /// The standard sprite scale of each segment.
        /// </summary>
        [DataField("staticScale")]
        public float StaticScale = 1f;

        /// <summary>
        /// Used to more finely tune how much damage should be transfered from tail to body.
        /// </summary>
        [DataField("damageModifierOffset")]
        public float DamageModifierOffset = 0.4f;

        /// <summary>
        /// A clamped variable that represents how far from the tip should tapering begin.
        /// </summary>
        [DataField("taperOffset")]
        public int TaperOffset = 18;

        /// <summary>
        /// Coefficient used to finely tune how much explosion damage should be transfered to the body. This is calculated multiplicatively with the derived damage modifier set.
        /// </summary>
        [DataField("explosiveModifierOffset")]
        public float ExplosiveModifierOffset = 0.1f;

        [DataField("bulletPassover")]
        public bool BulletPassover = true;
    }
}
