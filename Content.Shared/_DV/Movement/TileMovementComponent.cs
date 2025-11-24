using System.Numerics;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Movement;

/// <summary>
/// When attached to an entity with an InputMoverComponent, all mob movement on that entity will
/// be tile-based. Contains info used to facilitate that movement.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(TileMovementSystem))]
[AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class TileMovementComponent : Component
{
    /// <summary>
    /// Whether a tile movement slide is currently in progress.
    /// </summary>
    [ViewVariables]
    public bool SlideActive => MovementKeyPressedAt != null;

    /// <summary>
    /// Local coordinates from which the current slide first began.
    /// </summary>
    [AutoNetworkedField]
    public Vector2 Origin;

    /// <summary>
    /// Local coordinates of the target of the current slide.
    /// </summary>
    [AutoNetworkedField]
    public Vector2 Destination;

    /// <summary>
    /// This helps determine how long a slide should last. A slide will continue so long
    /// as a movement key (WASD) is being held down, but if it was held down for less than
    /// a certain time period then it will continue for a minimum period.
    /// </summary>
    [AutoNetworkedField]
    public TimeSpan? MovementKeyPressedAt;

    /// <summary>
    /// Move buttons used to initiate the current slide.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public MoveButtons CurrentSlideMoveButtons;

    /// <summary>
    /// Whether this entity was weightless last physics tick.
    /// </summary>
    [AutoNetworkedField]
    public bool WasWeightlessLastTick;

    /// <summary>
    /// Used to remove TileMovement after pulling stops.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Temporary;
}
