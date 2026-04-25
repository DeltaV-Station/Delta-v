using Content.Server.Actions;
using Content.Server.Ghost.Roles.Events;
using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.Actions;
using Content.Shared.Body.Part;
using Content.Shared.CombatMode;
using Content.Shared.Emp;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Pinpointer;
using Content.Shared.Popups;
using Content.Shared._DV.Replicator;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Server._DV.Replicator;

public sealed class ReplicatorSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly PinpointerSystem _pinpointer = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedReplicatorNestSystem _replicatorNest = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReplicatorComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<ReplicatorComponent, MindRemovedMessage>(OnMindRemoved);
        SubscribeLocalEvent<ReplicatorComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<ReplicatorComponent, ToggleCombatActionEvent>(OnCombatToggle);
        SubscribeLocalEvent<ReplicatorComponent, GhostRoleSpawnerUsedEvent>(OnGhostRoleSpawnerUsed);
        SubscribeLocalEvent<ReplicatorComponent, ReplicatorSpawnNestActionEvent>(OnSpawnNestAction);
        SubscribeLocalEvent<ReplicatorComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<ReplicatorComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<ReplicatorComponent, MapInitEvent>(OnReplicatorMapInit);
        SubscribeLocalEvent<ReplicatorComponent, BodyPartAddedEvent>(OnBodyPartAdded);
    }

    private void OnReplicatorMapInit(Entity<ReplicatorComponent> ent, ref MapInitEvent args)
    {
        CleanupInheritedHands(ent);
    }

    private void OnBodyPartAdded(Entity<ReplicatorComponent> ent, ref BodyPartAddedEvent args)
    {
        CleanupInheritedHands(ent);
    }

    private void OnMindAdded(Entity<ReplicatorComponent> ent, ref MindAddedMessage args)
    {
        CleanupInheritedHands(ent);

        if (ent.Comp.HasSpawnedNest)
            return;

        if (!ent.Comp.Queen)
            return;

        ent.Comp.Actions.Add(_actions.AddAction(ent, ent.Comp.SpawnNewNestAction));

        ent.Comp.HasSpawnedNest = true;
    }

    private void OnMindRemoved(Entity<ReplicatorComponent> ent, ref MindRemovedMessage args)
    {
        foreach (var action in ent.Comp.Actions)
        {
            QueueDel(action);
        }
    }

    private void OnSpawnNestAction(Entity<ReplicatorComponent> ent, ref ReplicatorSpawnNestActionEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var xform = Transform(ent);
        var coords = xform.Coordinates;
        if (!coords.IsValid(EntityManager) || xform.MapID == MapId.Nullspace)
            return;

        var myNest = Spawn("ReplicatorNest", xform.Coordinates);
        var myNestComp = EnsureComp<ReplicatorNestComponent>(myNest);

        if (ent.Comp.RelatedReplicators.Count <= 0 || ent.Comp.Queen && !ent.Comp.RelatedReplicators.Contains(ent))
            ent.Comp.RelatedReplicators.Add(ent);

        HashSet<EntityUid> newMinions = [];
        HashSet<(EntityUid, ReplicatorComponent)> livingReplicators = [];
        var query = EntityQueryEnumerator<ReplicatorComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            livingReplicators.Add((uid, comp));
        }

        foreach (var (uid, comp) in livingReplicators)
        {
            newMinions.Add(uid);

            if (_inventory.TryGetSlotEntity(uid, "pocket1", out var pocket1) && TryComp<PinpointerComponent>(pocket1, out var pinpointer))
                _pinpointer.SetTarget(pocket1.Value, myNest, pinpointer);

            var pinpointerQuery = EntityQueryEnumerator<PinpointerComponent, TransformComponent>();
            while (pinpointerQuery.MoveNext(out var pinUid, out var pinComp, out var pinXform))
            {
                if (pinXform.ParentUid == uid)
                    _pinpointer.SetTarget(pinUid, myNest, pinComp);
            }

            comp.MyNest = myNest;
        }

        myNestComp.SpawnedMinions = newMinions;
        myNestComp.SpawnedMinions.Add(ent);
        ent.Comp.MyNest = myNest;
        ent.Comp.RelatedReplicators.Clear();
        ent.Comp.Queen = false;

        _replicatorNest.ForceUpgrade(ent, ent.Comp.FirstStage);
    }

    private void OnGhostRoleSpawnerUsed(Entity<ReplicatorComponent> ent, ref GhostRoleSpawnerUsedEvent args)
    {
        CleanupInheritedHands(ent);

        if (!TryComp<SpawnedFromTrackerComponent>(args.Spawner, out var tracker) ||
            !TryComp<ReplicatorNestComponent>(tracker.SpawnedFrom, out var nestComp))
            return;

        nestComp.SpawnedMinions.Add(ent);
        nestComp.UnclaimedSpawners.Remove(args.Spawner);
        ent.Comp.MyNest = tracker.SpawnedFrom;
    }

    private void OnAttackAttempt(Entity<ReplicatorComponent> ent, ref AttackAttemptEvent args)
    {
        if (HasComp<ReplicatorComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("replicator-on-replicator-attack-fail"), ent, ent, PopupType.MediumCaution);
            args.Cancel();
        }

        if (HasComp<ReplicatorNestComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("replicator-on-nest-attack-fail"), ent, ent, PopupType.MediumCaution);
            args.Cancel();
        }
    }

    private void OnCombatToggle(Entity<ReplicatorComponent> ent, ref ToggleCombatActionEvent args)
    {
        if (!TryComp<CombatModeComponent>(ent, out var combat))
            return;

        _appearance.SetData(ent, ReplicatorVisuals.Combat, combat.IsInCombatMode);
    }

    private void OnMobStateChanged(Entity<ReplicatorComponent> ent, ref MobStateChangedEvent args)
    {
        if (_mobState.IsAlive(ent))
            return;

        _appearance.SetData(ent, ReplicatorVisuals.Combat, false);

        var query = EntityQueryEnumerator<ReplicatorComponent>();
        while (query.MoveNext(out var uid, out var replicatorComp))
        {
            _popup.PopupEntity(Loc.GetString(replicatorComp.QueenDiedMessage), uid, uid, PopupType.LargeCaution);
        }
    }

    private void OnEmpPulse(Entity<ReplicatorComponent> ent, ref EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;
        _stun.TryUpdateParalyzeDuration(ent, ent.Comp.EmpStunTime);
    }

    private void CleanupInheritedHands(Entity<ReplicatorComponent> ent)
    {
        if (!TryComp<HandsComponent>(ent, out var handsComp))
            return;

        foreach (var handId in handsComp.Hands.Keys.ToArray())
        {
            if (ent.Comp.Queen)
            {
                _hands.RemoveHand((ent.Owner, handsComp), handId);
                continue;
            }

            if (handId.Contains("-hand-"))
                continue;

            _hands.RemoveHand((ent.Owner, handsComp), handId);
        }
    }
}