using Content.Shared.Medical.Healing;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Medical.Healing;

/// <summary>
/// Allows a tool to be used as a healing item
/// With this component, the HealingSystem checks if the tool can be used before proceeding with healing
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(HealingSystem))]
public sealed partial class ToolHealingComponent : Component
{
    /// <summary>
    /// The amount of fuel used to heal
    /// </summary>
    [DataField]
    public float Fuel = 0.0f;
}
