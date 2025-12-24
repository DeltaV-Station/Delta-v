using Content.Shared.Physics;
using Robust.Shared.GameStates;
using Content.Shared.Weapons.Hitscan.Components;

namespace Content.Shared._DV.Weapons.Hitscan.Components;

/// <summary>
/// A component that will change the temperature of an entity hit with it.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HitscanTemperatureComponent : Component
{
    /// <summary>
    /// The amount of heat to transfer. Positive values add heat. Negative values subtract heat.
    /// </summary>
    [DataField]
    public float HeatChange = 0f;
}
