using Content.Shared.Medical.Healing;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Medical.Healing;

[RegisterComponent, NetworkedComponent]
[Access(typeof(HealingSystem))]
public sealed partial class ToolHealingComponent : Component
{
    [DataField]
    public float Fuel = 0.0f;
}
