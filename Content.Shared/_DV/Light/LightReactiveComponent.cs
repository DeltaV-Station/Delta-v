using Robust.Shared.GameStates;

namespace Content.Shared._DV.Light;

/// <summary>
/// A component that reacts to changes in light levels.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedLightReactiveSystem))]
public sealed partial class LightReactiveComponent : Component
{
    /// <summary>
    /// The frequency at which the component checks for light level changes.
    /// There should be very little reason it should be higher than this.
    /// </summary>
    [DataField]
    public TimeSpan UpdateFrequency = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Whether the component should only update while the entity is alive.
    /// If false, it will update even if the entity is dead.
    /// </summary>
    [DataField]
    public bool OnlyWhileAlive = true;

    /// <summary>
    /// Should this update its light level automatically, or only when asked to by another system?
    /// If true, it will update its light level automatically.
    /// If false, it will only update when explicitly requested.
    /// </summary>
    [DataField]
    public bool Manual = false;

    /// <summary>
    /// The next time the component should update.
    /// </summary>
    [AutoNetworkedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// The current light level of this entity.
    /// </summary>
    [DataField]
    public float CurrentLightLevel = 0f;
}
