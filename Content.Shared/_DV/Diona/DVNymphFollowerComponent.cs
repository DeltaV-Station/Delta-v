using Robust.Shared.GameStates;

namespace Content.Shared._DV.Diona;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(DVNymphRelationSystem))]
public sealed partial class DVNymphFollowerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Lead;
}

[ByRefEvent]
public readonly record struct DVNymphFollowerLeadGotChangedEvent;
