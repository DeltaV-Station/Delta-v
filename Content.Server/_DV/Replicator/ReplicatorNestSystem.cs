using System.Linq;
using Content.Server.Actions;
using Content.Server.Audio;
using Content.Server.Buckle.Systems;
using Content.Server.GameTicking;
using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Server.Storage.EntitySystems;
using Content.Server.Stunnable;
using Content.Shared.Actions;
using Content.Shared.Buckle.Components;
using Content.Shared.Destructible;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Pinpointer;
using Content.Shared.Popups;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Storage.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Whitelist;
using Content.Shared._DV.Replicator;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Replicator;

public sealed class ReplicatorNestSystem : SharedReplicatorNestSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedReplicatorNestSystem _sharedNest = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly MovementModStatusSystem _movementMod = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly PinpointerSystem _pinpointer = default!;
    [Dependency] private readonly AmbientSoundSystem _ambientSound = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly EntityStorageSystem _entStorage = default!;
    [Dependency] private readonly BuckleSystem _buckle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReplicatorNestComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ReplicatorNestComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<ReplicatorNestComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<ReplicatorNestComponent, StepTriggeredOffEvent>(OnStepTriggered);
        SubscribeLocalEvent<ReplicatorNestFallingComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<ReplicatorNestFallingComponent, PickupAttemptEvent>(OnFallingPickupAttempt);
        SubscribeLocalEvent<ReplicatorNestFallingComponent, GettingPickedUpAttemptEvent>(OnFallingGettingPickedUpAttempt);
        SubscribeLocalEvent<ReplicatorNestFallingComponent, PullAttemptEvent>(OnFallingPullAttempt);
        SubscribeLocalEvent<ReplicatorNestComponent, DestructionEventArgs>(OnDestroyed);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        HashSet<EntityUid> toDel = [];
        var query = EntityQueryEnumerator<ReplicatorNestFallingComponent>();
        while (query.MoveNext(out var uid, out var falling))
        {
            if (_timing.CurTime < falling.NextDeletionTime)
                continue;

            var nestComp = falling.FallingTarget.Comp;

            if (_whitelist.IsWhitelistPass(nestComp.PreservationBlacklist, uid))
            {
                toDel.Add(uid);
            }
            else if (!_whitelist.IsWhitelistPass(nestComp.PreservationWhitelist, uid))
            {
                if (!TryComp<MindContainerComponent>(uid, out var mindComp) || !mindComp.HasMind)
                    toDel.Add(uid);
            }

            _containerSystem.Insert(uid, falling.FallingTarget.Comp.Hole);
            EnsureComp<StunnedComponent>(uid);
            RemCompDeferred(uid, falling);
        }

        foreach (var uid in toDel)
        {
            QueueDel(uid);
        }
    }

    private void OnEntRemoved(Entity<ReplicatorNestComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        RemCompDeferred<StunnedComponent>(args.Entity);
    }

    private void OnMapInit(Entity<ReplicatorNestComponent> ent, ref MapInitEvent args)
    {
        if (!Transform(ent).Coordinates.IsValid(EntityManager))
            QueueDel(ent);

        ent.Comp.Hole = _containerSystem.EnsureContainer<Container>(ent, "hole");
        ent.Comp.NextSpawnAt = ent.Comp.SpawnNewAt;
        ent.Comp.NextUpgradeAt = ent.Comp.UpgradeAt;
        ent.Comp.NextTileConvertAt = ent.Comp.TileConvertAt;

        var pointsStorageEnt = Spawn("ReplicatorNestPointsStorage", Transform(ent).Coordinates);
        EnsureComp<ReplicatorNestPointsStorageComponent>(pointsStorageEnt);
        ent.Comp.PointsStorage = pointsStorageEnt;
    }

    private void OnStepTriggerAttempt(Entity<ReplicatorNestComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    private void OnStepTriggered(Entity<ReplicatorNestComponent> ent, ref StepTriggeredOffEvent args)
    {
        if (HasComp<ReplicatorNestFallingComponent>(args.Tripper))
            return;

        if (_whitelist.IsWhitelistPass(ent.Comp.Blacklist, args.Tripper))
        {
            if (TryComp<PullableComponent>(args.Tripper, out var pullable) && pullable.BeingPulled)
                _pulling.TryStopPull(args.Tripper, pullable);

            var xform = Transform(ent);
            var xformQuery = GetEntityQuery<TransformComponent>();
            var worldPos = _xform.GetWorldPosition(xform, xformQuery);
            var direction = _xform.GetWorldPosition(args.Tripper, xformQuery) - worldPos;
            _throwing.TryThrow(args.Tripper, direction * 10, 7, ent, 0);
            return;
        }

        var isReplicator = HasComp<ReplicatorComponent>(args.Tripper);
        if (TryComp<MobStateComponent>(args.Tripper, out var mobState) && isReplicator && _mobState.IsDead(args.Tripper))
        {
            _sharedNest.StartFalling(ent, args.Tripper);
            return;
        }

        if (mobState != null && _mobState.IsAlive(args.Tripper))
            return;

        if (TryComp<EntityStorageComponent>(args.Tripper, out var entStorage))
            _entStorage.EmptyContents(args.Tripper, entStorage);

        if (TryComp<StrapComponent>(args.Tripper, out var strapComp) && strapComp.BuckledEntities.Count > 0)
        {
            foreach (var buckled in strapComp.BuckledEntities)
            {
                if (!TryComp<BuckleComponent>(buckled, out var buckleComp))
                    continue;

                _buckle.Unbuckle((args.Tripper, buckleComp), null);
            }
        }

        _sharedNest.StartFalling(ent, args.Tripper);
    }

    private void OnUpdateCanMove(Entity<ReplicatorNestFallingComponent> ent, ref UpdateCanMoveEvent args)
    {
        args.Cancel();
    }

    private void OnFallingPickupAttempt(Entity<ReplicatorNestFallingComponent> ent, ref PickupAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnFallingGettingPickedUpAttempt(Entity<ReplicatorNestFallingComponent> ent, ref GettingPickedUpAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnFallingPullAttempt(Entity<ReplicatorNestFallingComponent> ent, ref PullAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnDestroyed(Entity<ReplicatorNestComponent> ent, ref DestructionEventArgs args)
    {
        HandleDestruction(ent);
    }

    private void HandleDestruction(Entity<ReplicatorNestComponent> ent)
    {
        if (TryComp<PointLightComponent>(ent.Comp.PointsStorage, out var lightComp))
            RemComp<PointLightComponent>(ent.Comp.PointsStorage);

        foreach (var uid in _containerSystem.EmptyContainer(ent.Comp.Hole))
        {
            RemCompDeferred<StunnedComponent>(uid);
            _stun.TryKnockdown(uid, TimeSpan.FromSeconds(2), false);
        }

        foreach (var spawner in ent.Comp.UnclaimedSpawners.ToArray())
        {
            ent.Comp.UnclaimedSpawners.Remove(spawner);
            QueueDel(spawner);
        }

        var fallingQuery = EntityQueryEnumerator<ReplicatorNestFallingComponent>();
        while (fallingQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.FallingTarget == ent)
                RemCompDeferred<ReplicatorNestFallingComponent>(uid);
        }

        EntityUid? queen = null;
        var livingReplicators = new HashSet<EntityUid>();
        var repQuery = EntityQueryEnumerator<ReplicatorComponent>();
        while (repQuery.MoveNext(out var uid, out var comp))
        {
            if (!_mobState.IsAlive(uid) || comp.MyNest != ent.Owner)
                continue;

            comp.MyNest = null;
            if (comp.Queen)
                queen = uid;

            livingReplicators.Add(uid);
        }

        if (livingReplicators.Count > 0)
        {
            var queenNotNull = queen ?? _random.Pick(livingReplicators);
            var queenComp = EnsureComp<ReplicatorComponent>(queenNotNull);
            queenComp.Queen = true;

            var related = new HashSet<Entity<ReplicatorComponent>>();
            foreach (var rep in livingReplicators)
            {
                if (TryComp<ReplicatorComponent>(rep, out var repComp))
                    related.Add((rep, repComp));
            }

            queenComp.RelatedReplicators = related;

            var upgradedQueen = ForceUpgrade((queenNotNull, queenComp), queenComp.FinalStage);
            if (upgradedQueen is { } upgradedQueenNotNull && TryComp<ReplicatorComponent>(upgradedQueenNotNull, out var upgradedComp))
            {
                queen = upgradedQueenNotNull;
                livingReplicators.Remove(queenNotNull);
                livingReplicators.Add(upgradedQueenNotNull);

                if (TryComp<MindContainerComponent>(upgradedQueenNotNull, out var mindContainer) && mindContainer.Mind is { } mind)
                {
                    if (!mindContainer.HasMind)
                        upgradedComp.Actions.Add(_actions.AddAction(upgradedQueenNotNull, upgradedComp.SpawnNewNestAction));
                    else
                        upgradedComp.Actions.Add(_actionContainer.AddAction(mind, upgradedComp.SpawnNewNestAction));
                }
            }
            else
            {
                queen = queenNotNull;
                if (TryComp<MindContainerComponent>(queenNotNull, out var mindContainer) && mindContainer.Mind is { } mind)
                {
                    if (!mindContainer.HasMind)
                        queenComp.Actions.Add(_actions.AddAction(queenNotNull, queenComp.SpawnNewNestAction));
                    else
                        queenComp.Actions.Add(_actionContainer.AddAction(mind, queenComp.SpawnNewNestAction));
                }
            }
        }

        foreach (var uid in livingReplicators)
        {
            if (!TryComp<ReplicatorComponent>(uid, out var comp))
                continue;

            var upgradedNotNull = uid == queen ? uid : ForceUpgrade((uid, comp), comp.FirstStage) ?? uid;

            _movementMod.TryUpdateMovementSpeedModDuration(upgradedNotNull, "HoleDestroyedSlowdownStatusEffect", TimeSpan.FromSeconds(3), 0.8f);

            if (_inventory.TryGetSlotEntity(upgradedNotNull, "pocket1", out var pocket1) &&
                TryComp<PinpointerComponent>(pocket1, out var pinpointer))
            {
                _pinpointer.SetTarget(pocket1.Value, queen, pinpointer);
            }

            var pinpointerQuery = EntityQueryEnumerator<PinpointerComponent, TransformComponent>();
            while (pinpointerQuery.MoveNext(out var pinUid, out var pinComp, out var pinXform))
            {
                if (pinXform.ParentUid == upgradedNotNull)
                    _pinpointer.SetTarget(pinUid, queen, pinComp);
            }

            _popup.PopupEntity(Loc.GetString("replicator-nest-destroyed"), uid, uid, PopupType.LargeCaution);
        }
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent args)
    {
        List<Entity<ReplicatorNestPointsStorageComponent>> nests = [];
        var query = AllEntityQuery<ReplicatorNestPointsStorageComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            nests.Add((uid, comp));
        }

        if (nests.Count == 0)
            return;

        args.AddLine(string.Empty);

        var totalPoints = 0;
        var totalSpawned = 0;
        HashSet<int> levels = [];
        var locationsList = string.Empty;
        var i = 0;
        foreach (var ent in nests)
        {
            i++;
            var pointsStorage = ent.Comp;
            var location = "Unknown";
            var mapCoords = _xform.ToMapCoordinates(Transform(ent).Coordinates);
            if (_navMap.TryGetNearestBeacon(mapCoords, out var beacon, out _) && beacon != null && beacon.Value.Comp.Text != null)
                location = beacon.Value.Comp.Text!;

            if (nests.Count == 1)
                locationsList = string.Concat(locationsList, "[color=#d70aa0]", location, "[/color].");
            else if (nests.Count == 2 && i == 1)
                locationsList = string.Concat(locationsList, "[color=#d70aa0]", location, " ");
            else if (i != nests.Count)
                locationsList = string.Concat(locationsList, "[color=#d70aa0]", location, "[/color], ");
            else
                locationsList = string.Concat(locationsList, "and [color=#d70aa0]", location, "[/color].");

            totalPoints += pointsStorage.TotalPoints / 10;
            totalSpawned += pointsStorage.TotalReplicators;
            levels.Add(pointsStorage.Level);
        }

        args.AddLine(Loc.GetString(
            "replicator-nest-end-of-round",
            ("location", locationsList),
            ("level", levels.Max()),
            ("points", totalPoints),
            ("replicators", totalSpawned)));
        args.AddLine(string.Empty);
    }
}