namespace Content.Server.Palmtree.Surgery
{
    [RegisterComponent]
    public partial class MindExchangerComponent : Component
    {
        [DataField("Mind")]
        [ViewVariables(VVAccess.ReadWrite)]
        public EntityUid mind = default!;

        [DataField("ContainsMind")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool occupied = false;
    }
}