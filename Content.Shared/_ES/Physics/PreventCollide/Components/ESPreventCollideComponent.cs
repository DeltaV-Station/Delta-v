using Robust.Shared.GameStates;

namespace Content.Shared._ES.Physics.PreventCollide.Components;

/// <summary>
/// Component that causes collisions with target entities to be excluded.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(ESPreventCollideSystem), Other = AccessPermissions.None)]
public sealed partial class ESPreventCollideComponent : Component
{
    /// <summary>
    /// Entities that this object will not collide with.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Entities = new();
}
