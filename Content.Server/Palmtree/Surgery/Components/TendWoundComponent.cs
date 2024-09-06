using Content.Shared.Damage;

namespace Content.Server.Palmtree.Surgery
{
    [RegisterComponent]
    public partial class PTendWoundsComponent : Component
    {
        [DataField("healThisMuch", required: true)] // Sorry I can't stop making silly names
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier healThisMuch = default!;
    }
}