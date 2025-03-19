using Robust.Shared.GameStates;

namespace Content.Server._DV.Tools;

[RegisterComponent, NetworkedComponent]
public sealed partial class PryingRequiresPowerComponent : Component
{
    [DataField]
    public float UseCost = 0;
}
