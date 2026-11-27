using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.MKC;

/// <summary>
/// starcup: Attached to a power core MKC organ to handle electricity-based hunger analogue for robots.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PowerCoreComponent : Component
{
    /// <summary>
    /// Time between each 'drink' from a battery
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// How many joules the 'drinker' drains from a power source per do-after
    /// </summary>
    [DataField, AutoNetworkedField]
    public float JoulesPerDrain = 192f;

    /// <summary>
    /// Multiply entity's movement speed by this amount when the entity is on low power.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float LowPowerMovementSpeedMultiplier = 0.75f;

    /// <summary>
    /// Standard power draw of an entity with this organ in watts (i.e. joules per second.)
    /// </summary>
    /// <remarks>
    /// My smartphone (a 2018 model) has a 3700 mAh battery @ 3.77 V and averages -270 mA with the screen on.
    ///
    /// 3700 mAh / 270 mA = 13.7 hour lifespan from full charge
    /// 3.7 Ah * 3.77 V = ~13.9 Wh
    /// 13.9 Wh / 13.7 hours = 1 W power draw
    ///
    /// A "hyper-capacity" power cell stores 1,800 joules and can thus power my smartphone for a mere thirty minutes.
    /// The power cell values are either fudged to create scarcity that fits expected round lengths or based on
    /// rechargeable battery technology from the early days of Space Station 13. This is bound to be something that
    /// distinctly dates this game over the coming decade.
    ///
    /// Conversely, the station AI has an internal battery of 300,000 joules with a 500 W draw. This is equivalent to
    /// a typical household kitchen refrigerator. A typical desktop computer draws 300 W, though this can be as low as
    /// 180 W. One must imagine that a full-size humanoid robot draws at least 40 W, though here we're forced to use a
    /// much smaller value for the sake of allowing the robots to carry around rechargeable snacks.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public float WattConsumption = 1;
}
