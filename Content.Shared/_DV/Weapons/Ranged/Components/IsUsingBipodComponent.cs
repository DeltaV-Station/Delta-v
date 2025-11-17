using Robust.Shared.GameStates;

namespace Content.Shared._DV.Weapons.Ranged.Components;

/// <summary>
/// An entity currently using a bipod weapon has this component.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IsUsingBipodComponent : Component
{
    /// <summary>
    /// A list of BipodComponents, so we can shut them down on movement.
    /// </summary>
    [AutoNetworkedField]
    public List<EntityUid> BipodOwnerUids = [];
}
