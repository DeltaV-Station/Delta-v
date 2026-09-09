using Robust.Shared.GameStates;

namespace Content.Shared._DV.Administration.Components;

/// This component prevents an entity from being followed by ghosts.
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class UnorbitableComponent : Component
{
    /// Whether to allow adminned players to follow the entity.
    [DataField, AutoNetworkedField]
    public bool AllowAdmins = false;
}
