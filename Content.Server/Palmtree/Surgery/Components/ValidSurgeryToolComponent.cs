using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server.Palmtree.Surgery
{
    [RegisterComponent]
    public partial class PSurgeryToolComponent : Component
    {
        [DataField("kind")]
        [ViewVariables(VVAccess.ReadWrite)]
        public string kind = "scalpel";

        [DataField("useDelay")]
        [ViewVariables(VVAccess.ReadWrite)]
        public float useDelay = 3.0f;

        [DataField("bleedAmountOnUse")]
        [ViewVariables(VVAccess.ReadWrite)]
        public float bleedAmountOnUse = 0.0f; // Cutting tools usually are the ones that cause bleed

        [DataField("damageOnUse", required: true)] // Tools damage the patient on use except in special cases.
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier damageOnUse = default!;

        [DataField("audioStart")]
        [ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier? audioStart = null;

        [DataField("audioEnd")]
        [ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier? audioEnd = null;
    }
}
