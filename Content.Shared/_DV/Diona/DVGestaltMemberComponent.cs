using Robust.Shared.GameStates;

namespace Content.Shared._DV.Diona;

[RegisterComponent, NetworkedComponent]
public sealed partial class DVGestaltMemberComponent : Component
{
    [DataField]
    public EntityUid? StoredInGestalt;
}
