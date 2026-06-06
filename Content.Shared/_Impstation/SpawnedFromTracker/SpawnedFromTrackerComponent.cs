using Robust.Shared.GameStates;

namespace Content.Shared._Impstation.SpawnedFromTracker;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpawnedFromTrackerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid SpawnedFrom;
}
