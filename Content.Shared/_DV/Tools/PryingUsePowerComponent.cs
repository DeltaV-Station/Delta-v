using Robust.Shared.GameStates;

namespace Content.Shared._DV.Tools;

[RegisterComponent, NetworkedComponent]
public sealed partial class PryingUsePowerComponent : Component
{
    [DataField]
    public float UseCost = 0;
}
