using Robust.Shared.GameStates;

namespace Content.Shared._DV.Replicator;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnedFromTrackerComponent : Component
{
    public EntityUid SpawnedFrom;
}
