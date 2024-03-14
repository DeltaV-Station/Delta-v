using Robust.Shared.Physics.Collision.Shapes;

namespace Content.Server.SizeAttribute
{
    [RegisterComponent]
    public sealed partial class SizeAttributeWhitelistComponent : Component
    {
        // Short
        [DataField("short")]
        public bool Short = false;

        [DataField("shortscale")]
        public float ShortScale = 0f;

        [DataField("shortDensity")]
        public float ShortDensity = 0f;

        [DataField("shortPseudoItem")]
        public bool ShortPseudoItem = false;

        [DataField("shortCosmeticOnly")]
        public bool ShortCosmeticOnly = true;

        // Delta-v: added custom pseudo-item shape
        /// <summary>
        /// An optional override for the shape of the item within the grid storage.
        /// If null, a default shape will be used based on <see cref="Size"/>.
        /// </summary>
        [DataField("pseudoItemShape")]
        public List<Box2i>? PseudoItemShape;

        // Tall
        [DataField("tall")]
        public bool Tall = false;

        [DataField("tallscale")]
        public float TallScale = 0f;

        [DataField("tallDensity")]
        public float TallDensity = 0f;

        [DataField("tallPseudoItem")]
        public bool TallPseudoItem = false;

        [DataField("tallCosmeticOnly")]
        public bool TallCosmeticOnly = true;
    }
}
