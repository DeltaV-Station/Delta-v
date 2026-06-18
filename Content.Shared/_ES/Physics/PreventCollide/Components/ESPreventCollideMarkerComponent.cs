using Robust.Shared.GameStates;

namespace Content.Shared._ES.Physics.PreventCollide.Components;

/// <summary>
/// Corresponding marker component to <see cref="ESPreventCollideMarkerComponent"/>
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(ESPreventCollideSystem), Other = AccessPermissions.None)]
public sealed partial class ESPreventCollideMarkerComponent : Component
{
    /// <summary>
    /// Entities that this object will not collide with.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Entities = new();
}
