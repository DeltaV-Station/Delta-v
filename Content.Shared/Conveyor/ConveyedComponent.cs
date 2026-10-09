using Robust.Shared.GameStates;

namespace Content.Shared.Conveyor;

/// <summary>
/// Indicates this entity is currently contacting a conveyor and will subscribe to events as appropriate.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ConveyedComponent : Component
{
    // TODO: Delete if pulling gets fixed.
    /// <summary>
    /// True if currently conveying.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Conveying;

    // Begin DeltaV - store CollisionWakeComponent state
    /// <summary>
    /// The value of CollisionWakeComponent.Enabled before the entity was put on the conveyor
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool WakeWasEnabled = true;
    // End DeltaV - store CollisionWakeComponent state
}
