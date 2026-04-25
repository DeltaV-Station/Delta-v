using Content.Shared.Maps;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Replicator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ReplicatorNestComponent : Component
{
    public readonly int MaxUpgradeStage = 2;

    public Container Hole = default!;

    [DataField]
    public EntityWhitelist Blacklist = new();

    [DataField]
    public EntityWhitelist PreservationWhitelist = new();

    [DataField]
    public EntityWhitelist PreservationBlacklist = new();

    [DataField(readOnly: true)]
    public int TotalPoints;

    [DataField(readOnly: true)]
    public int SpawningProgress;

    [DataField(readOnly: true), AutoNetworkedField]
    public int CurrentLevel = 1;

    [DataField]
    public int BonusPointsAlive = 10;

    [DataField]
    public int BonusPointsHumanoid;

    [DataField]
    public int TileConvertAt = 100;

    [DataField]
    public int SpawnNewAt = 300;

    [DataField]
    public int UpgradeAt = 400;

    [DataField]
    public int EndgameLevel = 3;

    [DataField]
    public int AnnounceAtLevel = 5;

    [DataField]
    public LocId Announcement = "replicator-level-warning";

    public bool HasAnnounced;

    [DataField]
    public float TileConversionChance = 0.05f;

    [DataField]
    public float TileConversionRadius = 1f;

    [DataField]
    public float TileConversionIncrease = 1f;

    [DataField]
    public EntProtoId ToSpawn = "SpawnPointGhostReplicator";

    [DataField]
    public EntProtoId SpawnNewNestAction = "ActionReplicatorSpawnNest";

    [DataField]
    public SoundSpecifier FallingSound = new SoundPathSpecifier("/Audio/_Impstation/Effects/falling.ogg");

    [DataField]
    public SoundSpecifier LevelUpSound = new SoundPathSpecifier("/Audio/_Impstation/Ambience/hole_2.ogg");

    [DataField]
    public SoundSpecifier UpgradeSound = new SoundPathSpecifier("/Audio/_Impstation/Misc/replicator_sfx2.ogg");

    [DataField]
    public SoundSpecifier TilePlaceSound = new SoundPathSpecifier("/Audio/_Impstation/Misc/replicator_sfx1.ogg");

    [DataField]
    public ProtoId<ContentTileDefinition> ConversionTile = "FloorReplicator";

    [DataField]
    public EntProtoId TileConversionVfx = "ReplicatorFloorSpawnVFX";

    public HashSet<EntityUid> SpawnedMinions = [];
    public HashSet<EntityUid> UnclaimedSpawners = [];
    public int NextSpawnAt;
    public int NextUpgradeAt;
    public int NextTileConvertAt;

    [DataField, AutoNetworkedField]
    public bool NeedsUpdate;

    public EntityUid PointsStorage;
}

[Serializable, NetSerializable]
public enum ReplicatorNestVisuals : byte
{
    Level1,
    Level2,
    Level3,
    Level1Unshaded,
    Level2Unshaded,
    Level3Unshaded,
}

[Serializable, NetSerializable]
public sealed partial class ReplicatorNestSizeChangedEvent : EntityEventArgs
{
}