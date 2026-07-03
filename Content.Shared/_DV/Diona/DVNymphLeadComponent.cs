using Robust.Shared.GameStates;

namespace Content.Shared._DV.Diona;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(DVNymphRelationSystem))]
public sealed partial class DVNymphLeadComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Followers = new();
}
