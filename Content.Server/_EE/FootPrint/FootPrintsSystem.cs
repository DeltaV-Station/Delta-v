using System.Linq; // DeltaV
using System.Numerics; // DeltaV
using Content.Server.Atmos.Components;
using Content.Shared._EE.FootPrint;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared._EE.FootPrint;
// using Content.Shared.Standing;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.GameTicking; // DeltaV
using Robust.Shared.Map;
using Robust.Shared.Map.Components; // DeltaV
using Robust.Shared.Random;

namespace Content.Server._EE.FootPrint;

public sealed class FootPrintsSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IMapManager _map = default!;

    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!; // DeltaV

    // DeltaV - Max amount of footprints per tile
    // Not stored on the component because its convenient here
    private const int MaxFootprintsPerTile = 2;

    /// <summary>
    ///     DeltaV: Dictionary tracking footprints per tile using tile coordinates as key
    /// </summary>
    private readonly Dictionary<Vector2i, Queue<EntityUid>> _footprintsPerTile = new();

    private EntityQuery<TransformComponent> _transformQuery;
    private EntityQuery<MobThresholdsComponent> _mobThresholdQuery;
    private EntityQuery<AppearanceComponent> _appearanceQuery;
//    private EntityQuery<LayingDownComponent> _layingQuery;

    public override void Initialize()
    {
        base.Initialize();

        _transformQuery = GetEntityQuery<TransformComponent>();
        _mobThresholdQuery = GetEntityQuery<MobThresholdsComponent>();
        _appearanceQuery = GetEntityQuery<AppearanceComponent>();
//      _layingQuery = GetEntityQuery<LayingDownComponent>();

        SubscribeLocalEvent<FootPrintsComponent, ComponentStartup>(OnStartupComponent);
        SubscribeLocalEvent<FootPrintsComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<FootPrintComponent, ComponentInit>(OnFootPrintInit); // DeltaV
        SubscribeLocalEvent<FootPrintComponent, ComponentRemove>(OnFootPrintRemove); // DeltaV
        SubscribeLocalEvent<MapGridComponent, EntityTerminatingEvent>(OnGridTerminating); // DeltaV
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnd); // DeltaV
    }

    private void OnStartupComponent(EntityUid uid, FootPrintsComponent component, ComponentStartup args)
    {
        component.StepSize = Math.Max(0f, component.StepSize + _random.NextFloat(-0.05f, 0.05f));
    }

    private void OnMove(EntityUid uid, FootPrintsComponent component, ref MoveEvent args)
    {
        if (component.PrintsColor.A <= 0f
            || !_transformQuery.TryComp(uid, out var transform)
            || !_mobThresholdQuery.TryComp(uid, out var mobThreshHolds)
            || !_map.TryFindGridAt(_transform.GetMapCoordinates((uid, transform)), out var gridUid, out _))
            return;

        var dragging = mobThreshHolds.CurrentThresholdState is MobState.Critical or MobState.Dead;
        var distance = (transform.LocalPosition - component.StepPos).Length();
        var stepSize = dragging ? component.DragSize : component.StepSize;

        if (!(distance > stepSize))
            return;

        component.RightStep = !component.RightStep;

        // DeltaV - Check if we've hit the footprint limit for this tile before spawning
        var coords = CalcCoords(gridUid, component, transform, dragging);
        if (!ShouldCreateNewFootprint(coords))
            return;

        var entity = Spawn(component.StepProtoId, coords);
        var footPrintComponent = EnsureComp<FootPrintComponent>(entity);

        footPrintComponent.PrintOwner = uid;
        Dirty(entity, footPrintComponent);

        if (_appearanceQuery.TryComp(entity, out var appearance))
        {
            _appearance.SetData(entity, FootPrintVisualState.State, PickState(uid, dragging), appearance);
            _appearance.SetData(entity, FootPrintVisualState.Color, component.PrintsColor, appearance);
        }

        if (!_transformQuery.TryComp(entity, out var stepTransform))
            return;

        stepTransform.LocalRotation = dragging
            ? (transform.LocalPosition - component.StepPos).ToAngle() + Angle.FromDegrees(-90f)
            : transform.LocalRotation + Angle.FromDegrees(180f);

        component.PrintsColor = component.PrintsColor.WithAlpha(Math.Max(0f, component.PrintsColor.A - component.ColorReduceAlpha));
        component.StepPos = transform.LocalPosition;

        if (!TryComp<SolutionContainerManagerComponent>(entity, out var solutionContainer)
            || !_solution.ResolveSolution((entity, solutionContainer), footPrintComponent.SolutionName, ref footPrintComponent.Solution, out var solution)
            || string.IsNullOrWhiteSpace(component.ReagentToTransfer) || solution.Volume >= 1)
            return;

        _solution.TryAddReagent(footPrintComponent.Solution.Value, component.ReagentToTransfer, 1, out _);
    }

    /// <summary>
    ///     DeltaV: Checks if a new footprint can be created at the specified coordinates based on the per-tile limit.
    /// </summary>
    /// <returns>True if a new footprint can be created, false if the tile is full or invalid</returns>
    private bool ShouldCreateNewFootprint(EntityCoordinates coords)
    {
        if (!coords.IsValid(EntityManager))
            return false;

        var mapCoords = _transform.ToMapCoordinates(coords);

        if (!_map.TryFindGridAt(mapCoords, out var gridUid, out var grid))
            return false;

        var tilePos = _mapSystem.CoordinatesToTile(gridUid, grid, coords);
        return !_footprintsPerTile.TryGetValue(tilePos, out var footprints) || footprints.Count < MaxFootprintsPerTile;
    }

    /// <summary>
    ///     DeltaV: Handles the initialization of a footprint component.
    /// </summary>
    private void OnFootPrintInit(Entity<FootPrintComponent> ent, ref ComponentInit args)
    {
        if (!TryGetTilePos(ent.Owner, out var tilePos))
            return;

        if (!_footprintsPerTile.TryGetValue(tilePos, out var footprints))
        {
            footprints = new Queue<EntityUid>();
            _footprintsPerTile[tilePos] = footprints;
        }

        footprints.Enqueue(ent);

        // If we've exceeded the limit, remove the oldest footprint
        if (footprints.Count > MaxFootprintsPerTile)
        {
            var oldestFootprint = footprints.Dequeue();
            if (Exists(oldestFootprint))
                QueueDel(oldestFootprint);
        }
    }

    /// <summary>
    ///     DeltaV: Handles cleanup when a footprint component is removed.
    /// </summary>
    private void OnFootPrintRemove(Entity<FootPrintComponent> ent, ref ComponentRemove args)
    {
        if (!TryGetTilePos(ent.Owner, out var tilePos))
            return;

        if (_footprintsPerTile.TryGetValue(tilePos, out var footprints))
        {
            // Create a new queue without the removed footprint
            var newQueue = new Queue<EntityUid>(footprints.Where(x => x != ent.Owner));
            if (newQueue.Count > 0)
                _footprintsPerTile[tilePos] = newQueue;
            else
                _footprintsPerTile.Remove(tilePos);
        }
    }

    /// <summary>
    ///     DeltaV: Handles cleanup when a grid is being terminated, removing all footprint tracking data from that grid.
    /// </summary>
    private void OnGridTerminating(Entity<MapGridComponent> ent, ref EntityTerminatingEvent args)
    {
        // Find and remove all footprints that belong to this grid's tiles
        var toRemove = new List<Vector2i>();

        foreach (var (pos, footprints) in _footprintsPerTile)
        {
            // Convert position to map coordinates to check if it belongs to this grid
            var mapCoords = _transform.ToMapCoordinates(new EntityCoordinates(ent,
                new Vector2(pos.X * ent.Comp.TileSize, pos.Y * ent.Comp.TileSize)));

            if (_map.TryFindGridAt(mapCoords, out var gridUid, out _) && gridUid == ent.Owner)
            {
                toRemove.Add(pos);
            }
        }

        foreach (var pos in toRemove)
        {
            _footprintsPerTile.Remove(pos);
        }
    }

    /// <summary>
    ///     DeltaV: Attempts to get the tile position for a given entity.
    /// </summary>
    private bool TryGetTilePos(EntityUid uid, out Vector2i tilePos)
    {
        tilePos = default;

        var coords = new EntityCoordinates(Transform(uid).ParentUid, Transform(uid).LocalPosition);
        var mapCoords = _transform.ToMapCoordinates(coords);

        if (!_map.TryFindGridAt(mapCoords, out var gridUid, out var grid))
            return false;

        tilePos = _mapSystem.CoordinatesToTile(gridUid, grid, coords);
        return true;
    }

    /// <summary>
    ///     DeltaV: Clean up the dict on round end
    /// </summary>
    private void OnRoundEnd(RoundRestartCleanupEvent ev)
    {
        _footprintsPerTile.Clear();
    }

    private EntityCoordinates CalcCoords(EntityUid uid, FootPrintsComponent component, TransformComponent transform, bool state)
    {
        if (state)
            return new EntityCoordinates(uid, transform.LocalPosition);

        var offset = component.RightStep
            ? new Angle(Angle.FromDegrees(180f) + transform.LocalRotation).RotateVec(component.OffsetPrint)
            : new Angle(transform.LocalRotation).RotateVec(component.OffsetPrint);

        return new EntityCoordinates(uid, transform.LocalPosition + offset);
    }

    private FootPrintVisuals PickState(EntityUid uid, bool dragging)
    {
        var state = FootPrintVisuals.BareFootPrint;

        if (_inventory.TryGetSlotEntity(uid, "shoes", out _))
            state = FootPrintVisuals.ShoesPrint;

        if (_inventory.TryGetSlotEntity(uid, "outerClothing", out var suit) && TryComp<PressureProtectionComponent>(suit, out _))
            state = FootPrintVisuals.SuitPrint;

        if (dragging)
            state = FootPrintVisuals.Dragging;

        return state;
    }
}
