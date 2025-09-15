using System.Numerics;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Displaces SS14 eye data when given to an entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class EyeCursorOffsetComponent : Component
{
    /// <summary>
    /// The amount the view will be displaced when the cursor is positioned at/beyond the max offset distance.
    /// Measured in tiles.
    /// </summary>
    [DataField]
    public float MaxOffset = 3f;

    /// <summary>
    /// The speed which the camera adjusts to new positions. 0.5f seems like a good value, but can be changed if you want very slow/instant adjustments.
    /// </summary>
    [DataField]
    public float OffsetSpeed = 0.5f;

    /// <summary>
    /// The amount the PVS should increase to account for the max offset.
    /// Should be 1/10 of MaxOffset most of the time.
    /// </summary>
    [DataField]
    public float PvsIncrease = 0.3f;

    /// <summary>
    /// The location the offset will attempt to pan towards; based on the cursor's position in the game window.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public Vector2 TargetPosition = Vector2.Zero;

    /// <summary>
    /// The current positional offset being applied. Used to enable gradual panning.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public Vector2 CurrentPosition = Vector2.Zero;

    [DataField]
    public bool Enabled = true;
}
