using Content.Server.Fluids.Components;
using Content.Server.Spreader;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Database;
using Content.Shared.Effects;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Content.Shared.Slippery;
using Content.Shared.Inventory;
using Content.Shared._Funkystation.Fluids;
using Robust.Shared.Collections;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Fluids.EntitySystems;

/// <summary>
/// Handles solutions on floors. Also handles the spreader logic for where the solution overflows a specified volume.
/// </summary>
public sealed partial class PuddleSystem : SharedPuddleSystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private EntityQuery<PuddleComponent> _puddleQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _puddleQuery = GetEntityQuery<PuddleComponent>();

        SubscribeLocalEvent<PuddleComponent, SpreadNeighborsEvent>(OnPuddleSpread);
        SubscribeLocalEvent<PuddleComponent, SlipEvent>(OnPuddleSlip);

        SubscribeLocalEvent<InventoryComponent, MoveEvent>(OnStepInPuddle);
    }

    private void OnStepInPuddle(Entity<InventoryComponent> ent, ref MoveEvent args)
    {
        var gridUid = args.NewPosition.GetGridUid(EntityManager);
        if (!gridUid.HasValue || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        if (args.OldPosition.GetGridUid(EntityManager) != gridUid ||
            _map.CoordinatesToTile(gridUid.Value, grid, args.OldPosition) == _map.CoordinatesToTile(gridUid.Value, grid, args.NewPosition))
            return;

        var tile = _map.GetTileRef(gridUid.Value, grid, args.NewPosition);

        if (!TryGetPuddle(tile, out var puddleUid) || !_puddleQuery.TryGetComponent(puddleUid, out var puddleComp))
            return;

        if (!_solutionContainerSystem.ResolveSolution(puddleUid, puddleComp.SolutionName, ref puddleComp.Solution, out var solution))
            return;

        if (solution.Volume <= FixedPoint2.Zero)
            return;

        var transferAmount = FixedPoint2.Min(FixedPoint2.New(1), solution.Volume);
        var splitSol = _solutionContainerSystem.SplitSolution(puddleComp.Solution.Value, transferAmount);

        if (_inventory.TryGetSlotEntity(ent.Owner, "shoes", out var shoes))
        {
            var spilledEvent = new SpilledOnEvent(puddleUid, splitSol);
            var relayedEvent = new InventoryRelayedEvent<SpilledOnEvent>(spilledEvent);
            RaiseLocalEvent(shoes.Value, relayedEvent);
        }
    }

    // TODO: This can be predicted once https://github.com/space-wizards/RobustToolbox/pull/5849 is merged
    private void OnPuddleSpread(Entity<PuddleComponent> entity, ref SpreadNeighborsEvent args)
    {
        // Overflow is the source of the overflowing liquid. This contains the excess fluid above overflow limit (20u)
        var overflow = GetOverflowSolution(entity.Owner, entity.Comp);

        if (overflow.Volume == FixedPoint2.Zero)
        {
            RemCompDeferred<ActiveEdgeSpreaderComponent>(entity);
            return;
        }

        // First we go to free tiles.
        if (args.NeighborFreeTiles.Count > 0 && args.Updates > 0)
        {
            _random.Shuffle(args.NeighborFreeTiles);
            var spillAmount = overflow.Volume / args.NeighborFreeTiles.Count;

            foreach (var neighbor in args.NeighborFreeTiles)
            {
                var split = overflow.SplitSolution(spillAmount);
                TrySpillAt(_map.GridTileToLocal(neighbor.Tile.GridUid, neighbor.Grid, neighbor.Tile.GridIndices), split, out _, false);
                args.Updates--;

                if (args.Updates <= 0)
                    break;
            }

            RemCompDeferred<ActiveEdgeSpreaderComponent>(entity);
            return;
        }

        // Then we overflow to neighbors with overflow capacity
        if (args.Neighbors.Count > 0)
        {
            var resolvedNeighbourSolutions = new ValueList<(Solution neighborSolution, PuddleComponent puddle, EntityUid neighbor)>();

            foreach (var neighbor in args.Neighbors)
            {
                if (!_puddleQuery.TryGetComponent(neighbor, out var puddle) ||
                    !_solutionContainerSystem.ResolveSolution(neighbor, puddle.SolutionName, ref puddle.Solution,
                        out var neighborSolution) ||
                    CanFullyEvaporate(neighborSolution))
                {
                    continue;
                }

                resolvedNeighbourSolutions.Add(
                    (neighborSolution, puddle, neighbor)
                );
            }

            resolvedNeighbourSolutions.Sort(
                (x, y) =>
                    x.neighborSolution.Volume.CompareTo(y.neighborSolution.Volume));

            foreach (var (neighborSolution, puddle, neighbor) in resolvedNeighbourSolutions)
            {
                if (neighborSolution.Volume >= (overflow.Volume + puddle.OverflowVolume))
                {
                    continue;
                }

                var remaining = puddle.OverflowVolume - neighborSolution.Volume;

                if (remaining <= FixedPoint2.Zero)
                    continue;

                if (neighborSolution.Volume + remaining >= (overflow.Volume + puddle.OverflowVolume))
                {
                    continue;
                }

                var split = overflow.SplitSolution(remaining);

                if (puddle.Solution != null && !_solutionContainerSystem.TryAddSolution(puddle.Solution.Value, split))
                    continue;

                args.Updates--;
                EnsureComp<ActiveEdgeSpreaderComponent>(neighbor);

                if (args.Updates <= 0)
                    break;
            }

            if (overflow.Volume == FixedPoint2.Zero)
            {
                RemCompDeferred<ActiveEdgeSpreaderComponent>(entity);
                return;
            }
        }

        if (overflow.Volume > FixedPoint2.Zero && args.Neighbors.Count > 0 && args.Updates > 0)
        {
            var resolvedNeighbourSolutions =
                new ValueList<(Solution neighborSolution, PuddleComponent puddle, EntityUid neighbor)>();

            FixedPoint2 totalVolume = 0;

            foreach (var neighbor in args.Neighbors)
            {
                if (!_puddleQuery.TryGetComponent(neighbor, out var puddle) ||
                    !_solutionContainerSystem.ResolveSolution(neighbor, puddle.SolutionName, ref puddle.Solution,
                        out var neighborSolution) ||
                    CanFullyEvaporate(neighborSolution))
                {
                    continue;
                }

                resolvedNeighbourSolutions.Add((neighborSolution, puddle, neighbor));
                totalVolume += neighborSolution.Volume;
            }

            resolvedNeighbourSolutions.Sort(
                (x, y) =>
                    x.neighborSolution.Volume.CompareTo(y.neighborSolution.Volume)
            );

            foreach (var (neighborSolution, puddle, neighbor) in resolvedNeighbourSolutions)
            {
                var sourceCurrentVolume = overflow.Volume + puddle.OverflowVolume;

                if (neighborSolution.Volume >= sourceCurrentVolume)
                {
                    continue;
                }

                var idealAverageVolume =
                    (totalVolume + overflow.Volume + puddle.OverflowVolume) / (args.Neighbors.Count + 1);

                if (idealAverageVolume > sourceCurrentVolume)
                {
                    continue;
                }

                var spillThisNeighbor = idealAverageVolume - neighborSolution.Volume;

                if (spillThisNeighbor < FixedPoint2.Zero)
                {
                    continue;
                }

                var split = overflow.SplitSolution(spillThisNeighbor);

                if (puddle.Solution != null && !_solutionContainerSystem.TryAddSolution(puddle.Solution.Value, split))
                    continue;

                EnsureComp<ActiveEdgeSpreaderComponent>(neighbor);
                args.Updates--;

                if (args.Updates <= 0)
                    break;
            }
        }

        if (_solutionContainerSystem.ResolveSolution(entity.Owner, entity.Comp.SolutionName, ref entity.Comp.Solution))
        {
            _solutionContainerSystem.TryAddSolution(entity.Comp.Solution.Value, overflow);
        }
    }

    private void OnPuddleSlip(Entity<PuddleComponent> entity, ref SlipEvent args)
    {
        if (!HasComp<ReactiveComponent>(args.Slipped) || HasComp<SlidingComponent>(args.Slipped))
            return;

        if (!_random.Prob(0.5f))
            return;

        if (!_solutionContainerSystem.ResolveSolution(entity.Owner, entity.Comp.SolutionName, ref entity.Comp.Solution, out var solution))
            return;

        Popups.PopupEntity(Loc.GetString("puddle-component-slipped-touch-reaction", ("puddle", entity.Owner)),
            args.Slipped, args.Slipped, PopupType.SmallCaution);

        var splitSol = _solutionContainerSystem.SplitSolution(entity.Comp.Solution.Value, solution.Volume * 0.15f);
        Reactive.DoEntityReaction(args.Slipped, splitSol, ReactionMethod.Touch);

        if (splitSol.Volume > 0)
        {
            var stainEv = new SpilledOnEvent(entity.Owner, splitSol.Clone());
            RaiseLocalEvent(args.Slipped, stainEv);
        }
    }

    public FixedPoint2 CurrentVolume(EntityUid uid, PuddleComponent? puddleComponent = null)
    {
        if (!Resolve(uid, ref puddleComponent))
            return FixedPoint2.Zero;

        return _solutionContainerSystem.ResolveSolution(uid, puddleComponent.SolutionName, ref puddleComponent.Solution,
            out var solution)
            ? solution.Volume
            : FixedPoint2.Zero;
    }

    public bool TryAddSolution(EntityUid puddleUid,
        Solution addedSolution,
        bool sound = true,
        bool checkForOverflow = true,
        PuddleComponent? puddleComponent = null,
        SolutionContainerManagerComponent? sol = null)
    {
        if (!Resolve(puddleUid, ref puddleComponent, ref sol))
            return false;

        _solutionContainerSystem.EnsureAllSolutions((puddleUid, sol));

        if (addedSolution.Volume == 0 ||
            !_solutionContainerSystem.ResolveSolution(puddleUid, puddleComponent.SolutionName,
                ref puddleComponent.Solution))
        {
            return false;
        }

        _solutionContainerSystem.AddSolution(puddleComponent.Solution.Value, addedSolution);

        if (checkForOverflow && IsOverflowing(puddleUid, puddleComponent))
        {
            EnsureComp<ActiveEdgeSpreaderComponent>(puddleUid);
        }

        if (!sound)
        {
            return true;
        }

        Audio.PlayPvs(puddleComponent.SpillSound, puddleUid);
        return true;
    }

    public bool WouldOverflow(EntityUid uid, Solution solution, PuddleComponent? puddle = null)
    {
        if (!Resolve(uid, ref puddle))
            return false;

        return CurrentVolume(uid, puddle) + solution.Volume > puddle.OverflowVolume;
    }

    private bool IsOverflowing(EntityUid uid, PuddleComponent? puddle = null)
    {
        if (!Resolve(uid, ref puddle))
            return false;

        return CurrentVolume(uid, puddle) > puddle.OverflowVolume;
    }

    public Solution GetOverflowSolution(EntityUid uid, PuddleComponent? puddle = null)
    {
        if (!Resolve(uid, ref puddle) ||
            !_solutionContainerSystem.ResolveSolution(uid, puddle.SolutionName, ref puddle.Solution))
        {
            return new Solution(0);
        }

        var remaining = puddle.OverflowVolume;
        var split = _solutionContainerSystem.SplitSolution(puddle.Solution.Value,
            CurrentVolume(uid, puddle) - remaining);
        return split;
    }

    #region Spill

    public override bool TrySplashSpillAt(Entity<SpillableComponent?> entity,
        EntityCoordinates coordinates,
        out EntityUid puddleUid,
        out Solution spilled,
        bool sound = true,
        EntityUid? user = null)
    {
        puddleUid = EntityUid.Invalid;
        spilled = new Solution();

        if (!Resolve(entity, ref entity.Comp))
            return false;

        if (!_solutionContainerSystem.TryGetSolution(entity.Owner, entity.Comp.SolutionName, out var solution))
            return false;

        spilled = solution.Value.Comp.Solution;

        return TrySplashSpillAt(entity, coordinates, solution.Value, out puddleUid, sound, user);
    }

    private bool TrySplashSpillAt(EntityUid entity,
        EntityCoordinates coordinates,
        Entity<SolutionComponent> solution,
        out EntityUid puddleUid,
        bool sound = true,
        EntityUid? user = null)
    {
        var result = TrySplashSpillAt(entity, coordinates, solution.Comp.Solution, out puddleUid, sound, user);
        _solutionContainerSystem.UpdateChemicals(solution);
        return result;
    }

    public override bool TrySplashSpillAt(EntityUid entity,
        EntityCoordinates coordinates,
        Solution solution,
        out EntityUid puddleUid,
        bool sound = true,
        EntityUid? user = null)
    {
        puddleUid = EntityUid.Invalid;

        if (solution.Volume == 0)
            return false;

        var spilled = solution.SplitSolution(solution.Volume);
        var targets = new List<EntityUid>();
        var reactive = new HashSet<Entity<ReactiveComponent>>();
        _lookup.GetEntitiesInRange(coordinates, 1.0f, reactive);

        foreach (var ent in reactive)
        {
            var owner = ent.Owner;
            var splitAmount = spilled.Volume * _random.NextFloat(0.05f, 0.30f);
            var splitSolution = spilled.SplitSolution(splitAmount);

            if (user != null)
            {
                AdminLogger.Add(LogType.Landed,
                    $"{ToPrettyString(user.Value):user} threw {ToPrettyString(entity):entity} which splashed a solution {SharedSolutionContainerSystem.ToPrettyString(spilled):solution} onto {ToPrettyString(owner):target}");
            }

            targets.Add(owner);
            Reactive.DoEntityReaction(owner, splitSolution, ReactionMethod.Touch);

            if (splitSolution.Volume > 0)
                RaiseLocalEvent(owner, new SpilledOnEvent(entity, splitSolution.Clone()));

            Popups.PopupEntity(Loc.GetString("spill-land-spilled-on-other",
                    ("spillable", entity),
                    ("target", Identity.Entity(owner, EntityManager))),
                owner,
                PopupType.SmallCaution);
        }

        _color.RaiseEffect(spilled.GetColor(_prototypeManager), targets,
            Filter.Pvs(entity, entityManager: EntityManager));

        return TrySpillAt(coordinates, spilled, out puddleUid, sound);
    }

    public override bool TrySpillAt(EntityCoordinates coordinates, Solution solution, out EntityUid puddleUid, bool sound = true)
    {
        if (solution.Volume == 0)
        {
            puddleUid = EntityUid.Invalid;
            return false;
        }

        var gridUid = _transform.GetGrid(coordinates);

        if (!TryComp<MapGridComponent>(gridUid, out var mapGrid))
        {
            puddleUid = EntityUid.Invalid;
            return false;
        }

        return TrySpillAt(_map.GetTileRef(gridUid.Value, mapGrid, coordinates), solution, out puddleUid, sound);
    }

    public override bool TrySpillAt(EntityUid uid, Solution solution, out EntityUid puddleUid, bool sound = true,
        TransformComponent? transformComponent = null)
    {
        if (!Resolve(uid, ref transformComponent, false))
        {
            puddleUid = EntityUid.Invalid;
            return false;
        }

        return TrySpillAt(transformComponent.Coordinates, solution, out puddleUid, sound: sound);
    }

    public override bool TrySpillAt(TileRef tileRef, Solution solution, out EntityUid puddleUid, bool sound = true,
        bool tileReact = true)
    {
        if (solution.Volume <= 0)
        {
            puddleUid = EntityUid.Invalid;
            return false;
        }

        if (tileRef.Tile.IsEmpty || _turf.IsSpace(tileRef))
        {
            puddleUid = EntityUid.Invalid;
            return false;
        }

        var gridId = tileRef.GridUid;
        if (!TryComp<MapGridComponent>(gridId, out var mapGrid))
        {
            puddleUid = EntityUid.Invalid;
            return false;
        }

        if (tileReact)
        {
            DoTileReactions(tileRef, solution);
        }

        if (solution.Volume == FixedPoint2.Zero)
        {
            puddleUid = EntityUid.Invalid;
            return false;
        }

        var anchored = _map.GetAnchoredEntitiesEnumerator(gridId, mapGrid, tileRef.GridIndices);
        var puddleQuery = GetEntityQuery<PuddleComponent>();
        var sparklesQuery = GetEntityQuery<EvaporationSparkleComponent>();

        while (anchored.MoveNext(out var ent))
        {
            if (sparklesQuery.TryGetComponent(ent, out var sparkles))
            {
                QueueDel(ent.Value);
                continue;
            }

            if (!puddleQuery.TryGetComponent(ent, out var puddle))
                continue;

            if (TryAddSolution(ent.Value, solution, sound, puddleComponent: puddle))
            {
                EnsureComp<ActiveEdgeSpreaderComponent>(ent.Value);
            }

            puddleUid = ent.Value;
            return true;
        }

        var coords = _map.GridTileToLocal(gridId, mapGrid, tileRef.GridIndices);
        puddleUid = Spawn("Puddle", coords);
        EnsureComp<PuddleComponent>(puddleUid);
        if (TryAddSolution(puddleUid, solution, sound))
        {
            EnsureComp<ActiveEdgeSpreaderComponent>(puddleUid);
        }

        return true;
    }

    #endregion

    public bool TryGetPuddle(TileRef tile, out EntityUid puddleUid)
    {
        puddleUid = EntityUid.Invalid;

        if (!TryComp<MapGridComponent>(tile.GridUid, out var grid))
            return false;

        var anc = _map.GetAnchoredEntitiesEnumerator(tile.GridUid, grid, tile.GridIndices);
        var puddleQuery = GetEntityQuery<PuddleComponent>();

        while (anc.MoveNext(out var ent))
        {
            if (!puddleQuery.HasComponent(ent.Value))
                continue;

            puddleUid = ent.Value;
            return true;
        }

        return false;
    }
}
