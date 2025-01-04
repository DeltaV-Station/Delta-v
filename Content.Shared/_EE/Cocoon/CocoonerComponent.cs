using Robust.Shared.GameStates;

namespace Content.Shared.Cocoon
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class CocoonerComponent : Component
    {
        [DataField("cocoonDelay")]
        public float CocoonDelay = 12f;

        [DataField("cocoonKnockdownMultiplier")]
        public float CocoonKnockdownMultiplier = 0.5f;
    }
}
