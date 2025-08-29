using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Floofstation.Leash.Components;

/// <summary>
///     Draws a line between this entity and the target. Same as JointVisualsComponent.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LeashedVisualsComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public SpriteSpecifier Sprite = default!;

    [DataField, AutoNetworkedField]
    public EntityUid Source, Target;

    [DataField, AutoNetworkedField]
    public Vector2 OffsetSource, OffsetTarget;
}
