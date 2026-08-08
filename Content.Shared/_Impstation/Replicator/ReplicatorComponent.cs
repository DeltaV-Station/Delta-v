using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Impstation.Replicator;

[RegisterComponent, NetworkedComponent]
public sealed partial class ReplicatorComponent : Component
{
    [DataField]
    public TimeSpan EmpStunTime = TimeSpan.FromSeconds(5);

    [DataField]
    public bool Queen;

    [DataField]
    public int UpgradeStage;

    public HashSet<Entity<ReplicatorComponent>> RelatedReplicators = [];

    public EntityUid? MyNest;

    [DataField]
    public HashSet<EntProtoId> UpgradeActions = [];

    [DataField]
    public string ReadyToUpgradeMessage = "replicator-upgrade-t1";

    [DataField]
    public EntProtoId SpawnNewNestAction = "ActionReplicatorSpawnNest";

    public HashSet<EntityUid?> Actions = [];

    public bool HasSpawnedNest;
    public bool HasBeenGivenUpgradeActions;

    [DataField]
    public LocId QueenDiedMessage = "replicator-queen-died-msg";

    [DataField]
    public EntProtoId FirstStage = "MobReplicator";

    [DataField]
    public EntProtoId FinalStage = "MobReplicatorTier3";
}

[Serializable, NetSerializable]
public enum ReplicatorVisuals : byte
{
    Combat
}
