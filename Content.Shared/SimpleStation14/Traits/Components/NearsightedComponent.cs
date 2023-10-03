using Robust.Shared.GameStates;

namespace Content.Shared.SimpleStation14.Traits.Components;

/// <summary>
///     Owner entity cannot see well, without prescription glasses.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NearsightedComponent : Component
{
    /// <summary>
    ///     Distance from the edge of the screen to the center
    /// </summary>
    /// <remarks>
    ///     I don't know how the distance is measured, 1 is very close to the center, 0 is maybe visible around the edge
    /// </remarks>
    [DataField("radius"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Radius = 0.8f;

    /// <summary>
    ///     How dark the circle mask is from <see cref="Radius"/>
    /// </summary>
    /// <remarks>
    ///     I also don't know how this works, it only starts getting noticeably dark at 0.7, and is definitely noticeable at 0.9, 1 is black
    /// </remarks>
    [DataField("alpha"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Alpha = 0.995f;

    /// <inheritdoc cref="Radius"/>
    [DataField("equippedRadius"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float EquippedRadius = 0.45f;

    /// <inheritdoc cref="Alpha"/>
    [DataField("equippedAlpha"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float EquippedAlpha = 0.93f;

    /// <summary>
    ///     How long the lerp animation should go on for in seconds.
    /// </summary>
    [DataField("lerpDuration"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float LerpDuration = 0.25f;

    /// <summary>
    ///     If true, uses the variables prefixed "Equipped"
    ///     If false, uses the variables without a prefix
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] // Make the system shared if you want this networked, I don't wanna do that
    public bool Active = false;
}
