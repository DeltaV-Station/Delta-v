using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared.Sprite;

/// <summary>
/// Used to set the <see cref="Robust.Client.GameObjects.SpriteComponent.Scale"/> datafield to a certain value from the server.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedScaleVisualsSystem))]
public sealed partial class ScaleVisualsComponent : Component
{
    /// <summary>
    /// The current sprite scale.
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables]
    public Vector2 Scale = Vector2.One;

    /// <summary>
    /// The original sprite scale, which we revert to if this component is removed.
    /// Only set on the client.
    /// </summary>
    [DataField]
    [ViewVariables]
    public Vector2? OriginalScale;

    /// <summary>
    /// DeltaV - Contains the species scale. Set dynamically by
    /// baseScale in the Species prototype.
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables]
    public Vector2 SpeciesScale = new(1f, 1f);

    /// <summary>
    /// DeltaV - Contains the user-defined scale from the character creation
    /// screen.
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables]
    public Vector2 ProfileScale = new(1f, 1f);

    /// <summary>
    /// DeltaV - Contains the computer scale from applying Scale, SpeciesScale, and ProfileScale.
    /// This will contain the actual scale after all modifiers are applied.
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables]
    public Vector2 ComputedScale;
}
