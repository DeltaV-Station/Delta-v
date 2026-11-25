using Content.Shared._DV.Body.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Body.Components;

/// <summary>
/// If an entity has this, if a small character has penalties (such as pull speed),
/// the small character will ignore the penalties associated with their size.
///
/// Mostly used for things like wheeled/floating objects.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(SmallCharacterSystem))]
public sealed partial class SmallCharacterComponent : Component
{
    /// <summary>
    /// The speed of which to scale the small character's pull speed by if the
    /// object is big enough to warrant a pull-speed slowdown.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float PullSpeedPenalty = 1f;
}
