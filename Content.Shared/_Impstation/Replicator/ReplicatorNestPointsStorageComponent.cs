using Robust.Shared.GameStates;

namespace Content.Shared._Impstation.Replicator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ReplicatorNestPointsStorageComponent : Component
{
    [DataField, AutoNetworkedField]
    public int TotalPoints;

    [DataField, AutoNetworkedField]
    public int TotalReplicators;

    [DataField, AutoNetworkedField]
    public int Level;
}
