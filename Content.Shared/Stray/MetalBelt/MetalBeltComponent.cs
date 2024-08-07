using Robust.Shared.GameStates;

namespace Content.Shared.Stray.MetalBelt;

[RegisterComponent]
[NetworkedComponent]
[Access(typeof(SharedMetalBeltSystem))]
public sealed partial class MetalBeltComponent : Component
{
    public bool IsWearing;
}
