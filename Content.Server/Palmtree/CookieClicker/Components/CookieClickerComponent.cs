namespace Content.Server.Palmtree.CookieClicker
{
    [RegisterComponent]
    public partial class ClickCounterComponent : Component
    {
        [DataField("count")]
        [ViewVariables(VVAccess.ReadWrite)]
        public int count = 0;
    }
}
