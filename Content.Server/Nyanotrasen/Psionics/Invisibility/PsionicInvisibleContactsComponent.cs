using Content.Shared.Whitelist;
using Robust.Shared.Timing;

namespace Content.Server.Psionics
{
    [RegisterComponent]
    public sealed partial class PsionicInvisibleContactsComponent : Component
    {
        [DataField("whitelist", required: true)]
        public EntityWhitelist Whitelist = default!;

        /// <summary>
        /// This tracks how many valid entities are being contacted,
        /// so when you stop touching one, you don't immediately lose invisibility.
        /// </summary>
        [DataField("stages")]
        public int Stages = 0;
    }
}
