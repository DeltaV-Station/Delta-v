using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Audio;
using Content.Shared.Construction.Components;
using Content.Shared.Humanoid;
using Content.Shared.Item;
using Content.Shared.Maps;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Interaction.Components;
using Content.Shared.Stacks;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Whitelist;
using Content.Shared._Impstation.SpawnedFromTracker;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Impstation.Replicator;

public abstract class SharedReplicatorNestSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSound = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entStorage = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReplicatorComponent, ReplicatorUpgradeActionEvent>(OnUpgrade);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsClient)
            return;

        var query = EntityQueryEnumerator<ReplicatorNestComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.NeedsUpdate)
                continue;

            Embiggen((uid, comp));
            comp.NeedsUpdate = false;
        }
    }

    public void StartFalling(Entity<ReplicatorNestComponent> ent, EntityUid tripper, bool playSound = true)
    {
        HandlePoints(ent, tripper);

        if (TryComp<PullableComponent>(tripper, out var pullable) && pullable.BeingPulled)
            _pulling.TryStopPull(tripper, pullable);

        var fall = EnsureComp<ReplicatorNestFallingComponent>(tripper);
        fall.FallingTarget = ent;
        fall.NextDeletionTime = _timing.CurTime + fall.DeletionTime;
        _stun.TryKnockdown(tripper, fall.DeletionTime, false);

        if (playSound)
            _audio.PlayPvs(ent.Comp.FallingSound, tripper);
    }

    private void HandlePoints(Entity<ReplicatorNestComponent> ent, EntityUid tripper)
    {
        if (!HasComp<StackComponent>(tripper))
        {
            ent.Comp.TotalPoints += 10;
            ent.Comp.SpawningProgress += 10;
        }

        if (TryComp<StackComponent>(tripper, out var stackComp))
        {
            ent.Comp.TotalPoints += stackComp.Count;
            ent.Comp.SpawningProgress += stackComp.Count;
        }
        else if (TryComp<ItemComponent>(tripper, out var itemComp))
        {
            if (_item.GetSizePrototype(itemComp.Size) == _item.GetSizePrototype("Large"))
                ent.Comp.TotalPoints += 10;
            else if (_item.GetSizePrototype(itemComp.Size) == _item.GetSizePrototype("Huge"))
                ent.Comp.TotalPoints += 20;
            else if (_item.GetSizePrototype(itemComp.Size) >= _item.GetSizePrototype("Ginormous"))
                ent.Comp.TotalPoints += 30;

            ent.Comp.SpawningProgress += 10;
        }
        else if (TryComp<AnchorableComponent>(tripper, out _))
        {
            ent.Comp.TotalPoints += 30;
            ent.Comp.SpawningProgress += 30;
        }
        else if (HasComp<ReplicatorComponent>(tripper))
        {
            ent.Comp.SpawningProgress += ent.Comp.SpawnNewAt / 4;
        }
        else if (HasComp<MobStateComponent>(tripper))
        {
            if (HasComp<HumanoidAppearanceComponent>(tripper))
            {
                ent.Comp.TotalPoints += ent.Comp.BonusPointsHumanoid * ent.Comp.CurrentLevel;
                ent.Comp.SpawningProgress += ent.Comp.SpawnNewAt;
            }
            else
            {
                ent.Comp.TotalPoints += ent.Comp.BonusPointsAlive * ent.Comp.CurrentLevel;
                ent.Comp.SpawningProgress += ent.Comp.SpawnNewAt / 4;
            }
        }

        if (ent.Comp.TotalPoints >= ent.Comp.NextUpgradeAt)
        {
            ent.Comp.CurrentLevel++;

            var growthMessage = $"replicator-nest-level{ent.Comp.CurrentLevel}";
            if (Loc.TryGetString(growthMessage, out var localizedMsg))
                _popup.PopupEntity(localizedMsg, ent);
            else
                _popup.PopupEntity(Loc.GetString("replicator-nest-levelup"), ent);

            if (ent.Comp.CurrentLevel <= ent.Comp.EndgameLevel)
                ent.Comp.NeedsUpdate = true;

            ent.Comp.NextUpgradeAt += ent.Comp.CurrentLevel >= ent.Comp.EndgameLevel
                ? ent.Comp.UpgradeAt * ent.Comp.EndgameLevel
                : ent.Comp.UpgradeAt * ent.Comp.CurrentLevel;

            UpgradeAll(ent);
            _audio.PlayPvs(ent.Comp.LevelUpSound, ent);

            ent.Comp.TileConversionRadius += ent.Comp.TileConversionIncrease;

            if (TryComp<AmbientSoundComponent>(ent.Comp.PointsStorage, out var ambientComp))
                _ambientSound.SetRange(ent.Comp.PointsStorage, ambientComp.Range + 1, ambientComp);
        }

        if (ent.Comp.SpawningProgress >= ent.Comp.NextSpawnAt)
        {
            SpawnNew(ent);
            ent.Comp.NextSpawnAt += ent.Comp.SpawnNewAt * ent.Comp.UnclaimedSpawners.Count;
        }

        if (ent.Comp.TotalPoints >= ent.Comp.NextTileConvertAt && ent.Comp.CurrentLevel > ent.Comp.EndgameLevel)
        {
            ConvertTiles(ent, ent.Comp.TileConversionRadius);
            ent.Comp.NextTileConvertAt += ent.Comp.TileConvertAt;
        }

        Dirty(ent);

        if (!TryComp<ReplicatorNestPointsStorageComponent>(ent.Comp.PointsStorage, out var pointsStorageComponent))
            pointsStorageComponent = EnsureComp<ReplicatorNestPointsStorageComponent>(ent.Comp.PointsStorage);

        pointsStorageComponent.Level = ent.Comp.CurrentLevel;
        pointsStorageComponent.TotalPoints = ent.Comp.TotalPoints;
        pointsStorageComponent.TotalReplicators = ent.Comp.SpawnedMinions.Count;
    }

    private void SpawnNew(Entity<ReplicatorNestComponent> ent)
    {
        if (_net.IsClient)
            return;

        var spawner = Spawn(ent.Comp.ToSpawn, Transform(ent).Coordinates);
        var tracker = EnsureComp<SpawnedFromTrackerComponent>(spawner);
        tracker.SpawnedFrom = ent;
        ent.Comp.UnclaimedSpawners.Add(spawner);
    }

    public void UpgradeAll(Entity<ReplicatorNestComponent> ent)
    {
        if (_net.IsClient || !_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<ReplicatorComponent>();
        while (query.MoveNext(out var uid, out var replicatorComp))
        {
            if (replicatorComp.UpgradeActions.Count == 0 || replicatorComp.HasBeenGivenUpgradeActions)
                continue;

            foreach (var action in replicatorComp.UpgradeActions)
            {
                replicatorComp.Actions.Add(_actions.AddAction(uid, action));
            }

            replicatorComp.HasBeenGivenUpgradeActions = true;
        }
    }

    public EntityUid? ForceUpgrade(Entity<ReplicatorComponent> ent, EntProtoId nextStage)
    {
        if (_net.IsClient || !_timing.IsFirstTimePredicted)
            return null;

        var upgraded = UpgradeReplicator(ent, nextStage);

        QueueDel(ent);
        foreach (var action in ent.Comp.Actions)
        {
            QueueDel(action);
        }

        return upgraded;
    }

    public void OnUpgrade(Entity<ReplicatorComponent> ent, ref ReplicatorUpgradeActionEvent args)
    {
        if (_net.IsClient || !_timing.IsFirstTimePredicted)
            return;

        if (ent.Comp.MyNest == null || UpgradeReplicator(ent, args.NextStage) == null)
        {
            _popup.PopupEntity(Loc.GetString("replicator-cant-find-nest"), ent, PopupType.MediumCaution);
            return;
        }

        QueueDel(ent);
        foreach (var action in ent.Comp.Actions)
        {
            QueueDel(action);
        }

        _popup.PopupEntity(Loc.GetString($"{ent.Comp.ReadyToUpgradeMessage}-others", ("replicator", ent)), ent, PopupType.MediumCaution);
    }

    public EntityUid? UpgradeReplicator(Entity<ReplicatorComponent> ent, EntProtoId nextStage)
    {
        if (!_mind.TryGetMind(ent, out var mind, out _))
            return null;

        var xform = Transform(ent);
        var upgraded = Spawn(nextStage, xform.Coordinates);
        var upgradedComp = EnsureComp<ReplicatorComponent>(upgraded);
        upgradedComp.RelatedReplicators = ent.Comp.RelatedReplicators;
        upgradedComp.MyNest = ent.Comp.MyNest;
        upgradedComp.Actions = new HashSet<EntityUid?>(ent.Comp.Actions);
        upgradedComp.HasBeenGivenUpgradeActions = false; // Reset so new tier gets its own upgrade actions

        if (ent.Comp.MyNest != null)
        {
            var nestComp = EnsureComp<ReplicatorNestComponent>((EntityUid) ent.Comp.MyNest);
            nestComp.SpawnedMinions.Remove(ent);
            nestComp.SpawnedMinions.Add(upgraded);
            _audio.PlayPvs(nestComp.UpgradeSound, upgraded);
        }

        _mind.TransferTo(mind, upgraded);
        _popup.PopupEntity(Loc.GetString($"{ent.Comp.ReadyToUpgradeMessage}-self"), upgraded, PopupType.Medium);

        return upgraded;
    }

    private void Embiggen(Entity<ReplicatorNestComponent> ent)
    {
        var ev = new ReplicatorNestEmbiggenedEvent(ent);
        RaiseLocalEvent(ent, ref ev);
    }

    private void ConvertTiles(Entity<ReplicatorNestComponent> ent, float radius)
    {
        var xform = Transform(ent);
        if (xform.GridUid is not { } gridUid || !TryComp(gridUid, out MapGridComponent? mapGrid))
            return;

        var tileEnumerator = _map.GetLocalTilesEnumerator(
            gridUid,
            mapGrid,
            new Box2(
                xform.Coordinates.Position + new System.Numerics.Vector2(-radius, -radius),
                xform.Coordinates.Position + new System.Numerics.Vector2(radius, radius)));
        var convertTile = (ContentTileDefinition) _tileDef[ent.Comp.ConversionTile];

        while (tileEnumerator.MoveNext(out var tile))
        {
            if (tile.Tile.TypeId == convertTile.TileId)
                continue;

            var tileCoords = tile.GridIndices;
            var nestCoords = xform.Coordinates.Position;
            if (Math.Sqrt(Math.Pow(tileCoords.X - (nestCoords.X - 0.5), 2) + Math.Pow(tileCoords.Y - (nestCoords.Y - 0.5), 2)) >= radius)
                continue;

            if (!_random.Prob(ent.Comp.TileConversionChance))
                continue;

            var center = _turf.GetTileCenter(tile);
            Spawn(ent.Comp.TileConversionVfx, center);
            _audio.PlayPvs(ent.Comp.TilePlaceSound, center);
            _tile.ReplaceTile(tile, convertTile);
            _tile.PickVariant(convertTile);
        }
    }
}

public sealed partial class ReplicatorSpawnNestActionEvent : InstantActionEvent
{
}

public sealed partial class ReplicatorUpgradeActionEvent : InstantActionEvent
{
    [DataField(required: true)]
    public EntProtoId NextStage;
}

[ByRefEvent]
public sealed partial class ReplicatorNestEmbiggenedEvent(Entity<ReplicatorNestComponent> ent) : EntityEventArgs
{
    public Entity<ReplicatorNestComponent> Ent { get; set; } = ent;
}
