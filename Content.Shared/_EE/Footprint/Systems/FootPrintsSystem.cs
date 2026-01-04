using Content.Shared._EE.Flight;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Standing;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Configuration;
using Content.Shared._DV.CCVars;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared._EE.FootPrint.Systems;

/// <summary>
/// Handles creation of footprints as entities move.
/// </summary>
public sealed partial class FootPrintsSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedFlightSystem _flight = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private EntityQuery<AppearanceComponent> _appearanceQuery;
    private EntityQuery<FlightComponent> _flightQuery;
    private EntityQuery<GridFootPrintsComponent> _gridFootPrintsQuery;
    private EntityQuery<MobThresholdsComponent> _mobThresholdQuery;
    private EntityQuery<StandingStateComponent> _standingQuery;
    private EntityQuery<TransformComponent> _transformQuery;

    private const string HardsuitTag = "Hardsuit";

    private int _maxPerTile;
    private int _maxPerGrid;

    public override void Initialize()
    {
        base.Initialize();

        _appearanceQuery = GetEntityQuery<AppearanceComponent>();
        _flightQuery = GetEntityQuery<FlightComponent>();
        _gridFootPrintsQuery = GetEntityQuery<GridFootPrintsComponent>();
        _mobThresholdQuery = GetEntityQuery<MobThresholdsComponent>();
        _standingQuery = GetEntityQuery<StandingStateComponent>();
        _transformQuery = GetEntityQuery<TransformComponent>();

        SubscribeLocalEvent<FootPrintsComponent, ComponentStartup>(OnStartupComponent);
        SubscribeLocalEvent<FootPrintsComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<FootPrintComponent, ComponentRemove>(OnFootPrintRemoved);

        InitializeStateHandling();

        // Subscribe to CVar changes
        Subs.CVar(_cfg, DCCVars.MaxFootPrintsPerTile, value => _maxPerTile = value, true);
        Subs.CVar(_cfg, DCCVars.MaxFootPrintsPerGrid, value => _maxPerGrid = value, true);
    }

    private void OnStartupComponent(Entity<FootPrintsComponent> ent, ref ComponentStartup args)
    {
        // Add slight random variation to step size for more natural-looking prints
        ent.Comp.StepSize = Math.Max(0f, ent.Comp.StepSize + _random.NextFloat(-0.05f, 0.05f));
        DirtyField(ent.AsNullable(), nameof(FootPrintsComponent.StepSize));
    }

    private void OnMove(Entity<FootPrintsComponent> ent, ref MoveEvent args)
    {
        // Don't create footprints if flying
        if (_flightQuery.TryComp(ent, out var flight) && _flight.IsFlying((ent, flight)))
            return;

        // Don't create footprints if color is fully transparent
        if (ent.Comp.PrintsColor.A <= 0f)
            return;

        if (!_transformQuery.TryComp(ent, out var transform))
            return;

        if (!_mobThresholdQuery.TryComp(ent, out var mobThresholds))
            return;

        // Check if entity is being dragged (critical or dead)
        var isCrit = mobThresholds.CurrentThresholdState is MobState.Critical or MobState.Dead;
        var dragging = isCrit || _standingQuery.TryComp(ent, out var standingComp) && _standing.IsDown((ent, standingComp));

        // Calculate distance traveled since last footprint
        var distance = (transform.LocalPosition - ent.Comp.StepPos).Length();
        var stepSize = dragging ? ent.Comp.DragSize : ent.Comp.StepSize;

        // Not enough distance traveled yet - MOST COMMON EARLY RETURN
        if (distance <= stepSize)
            return;

        // Need to be on a grid to leave footprints
        if (!_map.TryFindGridAt(_transform.GetMapCoordinates((ent, transform)), out var gridUid, out var grid))
            return;

        // Calculate spawn coordinates
        var coords = CalcCoords(gridUid, ent.Comp, transform, dragging);

        // Get the tile position for limit checking
        if (!_mapSystem.TryGetTileRef(gridUid, grid, coords, out var tileRef))
            return;

        var tilePos = tileRef.GridIndices;

        // Check per-tile limit BEFORE spawning
        // Use TryComp instead of EnsureComp to avoid adding component in hot path
        if (_maxPerTile > 0 && _gridFootPrintsQuery.TryComp(gridUid, out var gridFootPrints))
        {
            if (gridFootPrints.FootPrintsByTile.TryGetValue(tilePos, out var existingFootPrints)
                && existingFootPrints.Count >= _maxPerTile)
            {
                // Tile is full, don't spawn
                return;
            }
        }

        // Alternate feet
        ent.Comp.RightStep = !ent.Comp.RightStep;
        DirtyField(ent.AsNullable(), nameof(FootPrintsComponent.RightStep));

        // Spawn the footprint entity
        var footprint = EntityManager.PredictedSpawnAtPosition(ent.Comp.StepProtoId.Id, coords);

        // Update appearance with current color and state
        if (_appearanceQuery.TryComp(footprint, out var appearance))
        {
            _appearance.SetData(footprint, FootPrintVisualState.State, PickState(ent, dragging), appearance);
            _appearance.SetData(footprint, FootPrintVisualState.Color, ent.Comp.PrintsColor, appearance);
        }

        // Set rotation
        if (_transformQuery.TryComp(footprint, out var stepTransform))
        {
            stepTransform.LocalRotation = dragging
                ? (transform.LocalPosition - ent.Comp.StepPos).ToAngle() + Angle.FromDegrees(-90f)
                : transform.LocalRotation + Angle.FromDegrees(180f);
        }

        if (!TryComp<FootPrintComponent>(footprint, out var footPrintComponent))
            return;

        // Set the owner reference
        footPrintComponent.PrintOwner = ent;
        Dirty(footprint, footPrintComponent);

        // Track the footprint on the grid
        TrackFootPrint(gridUid, GetNetEntity(footprint), tilePos);

        // Reduce color alpha for next footprint
        ent.Comp.PrintsColor = ent.Comp.PrintsColor.WithAlpha(
            Math.Max(0f, ent.Comp.PrintsColor.A - ent.Comp.ColorReduceAlpha));
        DirtyField(ent.AsNullable(), nameof(FootPrintsComponent.PrintsColor));

        // Update last step position
        ent.Comp.StepPos = transform.LocalPosition;
        DirtyField(ent.AsNullable(), nameof(FootPrintsComponent.StepPos));

        // Handle reagent transfer
        if (ent.Comp.ReagentToTransfer is { } reagent)
        {
            TryTransferReagent((footprint, footPrintComponent), ent, reagent);
        }
    }

    private void OnFootPrintRemoved(Entity<FootPrintComponent> ent, ref ComponentRemove args)
    {
        // Clean up tracking when footprint is deleted
        if (!_transformQuery.TryComp(ent, out var transform))
            return;

        if (transform.GridUid == null)
            return;

        UntrackFootPrint(transform.GridUid.Value, GetNetEntity(ent));
    }

    private void TrackFootPrint(EntityUid gridUid, NetEntity footPrintUid, Vector2i tile)
    {
        var gridFootPrints = EnsureComp<GridFootPrintsComponent>(gridUid);

        // Add to tile tracking
        if (!gridFootPrints.FootPrintsByTile.TryGetValue(tile, out var tileFootPrints))
        {
            tileFootPrints = new List<NetEntity>();
            gridFootPrints.FootPrintsByTile[tile] = tileFootPrints;
        }

        tileFootPrints.Add(footPrintUid);
        gridFootPrints.TotalFootPrints++;

        // DELTA TRACKING: Mark tile as dirty
        gridFootPrints.DirtyTiles.Add(tile);
        // Remove from removed set if it was there
        gridFootPrints.RemovedTiles.Remove(tile);

        // Enforce global limit
        if (_maxPerGrid > 0 && gridFootPrints.TotalFootPrints > _maxPerGrid)
        {
            RemoveOldestFootPrint(gridFootPrints);
        }

        Dirty(gridUid, gridFootPrints);
    }

    private void UntrackFootPrint(EntityUid gridUid, NetEntity footPrintUid)
    {
        if (!_gridFootPrintsQuery.TryComp(gridUid, out var gridFootPrints))
            return;

        // Find and remove from tile tracking
        foreach (var (tile, footPrints) in gridFootPrints.FootPrintsByTile)
        {
            if (footPrints.Remove(footPrintUid))
            {
                gridFootPrints.TotalFootPrints--;

                // DELTA TRACKING
                if (footPrints.Count == 0)
                {
                    // Tile is now empty
                    gridFootPrints.FootPrintsByTile.Remove(tile);
                    gridFootPrints.RemovedTiles.Add(tile);
                    gridFootPrints.DirtyTiles.Remove(tile);
                }
                else
                {
                    // Tile still has footprints, just modified
                    gridFootPrints.DirtyTiles.Add(tile);
                }

                Dirty(gridUid, gridFootPrints);
                break;
            }
        }
    }

    private void RemoveOldestFootPrint(GridFootPrintsComponent gridFootPrints)
    {
        foreach (var (tile, footPrints) in gridFootPrints.FootPrintsByTile)
        {
            if (footPrints.Count > 0)
            {
                var toRemove = footPrints[0];
                footPrints.RemoveAt(0);
                QueueDel(GetEntity(toRemove));
                gridFootPrints.TotalFootPrints--;

                // DELTA TRACKING
                if (footPrints.Count == 0)
                {
                    gridFootPrints.FootPrintsByTile.Remove(tile);
                    gridFootPrints.RemovedTiles.Add(tile);
                    gridFootPrints.DirtyTiles.Remove(tile);
                }
                else
                {
                    gridFootPrints.DirtyTiles.Add(tile);
                }

                return;
            }
        }
    }

    private void TryTransferReagent(Entity<FootPrintComponent> ent, Entity<FootPrintsComponent> tripper, ProtoId<ReagentPrototype> reagentId)
    {
        if (!TryComp<SolutionContainerManagerComponent>(ent, out var solutionContainer))
            return;

        if (!_solution.ResolveSolution((ent, solutionContainer),
                ent.Comp.SolutionName,
            ref ent.Comp.Solution,
                out var solution))
            return;

        // Don't overfill
        if (solution.Volume >= 1)
            return;

        // Transfer a small amount of reagent
        _solution.TryAddReagent(ent.Comp.Solution.Value, reagentId, tripper.Comp.AmountToTransfer, out _);
    }

    private EntityCoordinates CalcCoords(EntityUid gridUid,
        FootPrintsComponent component,
        TransformComponent transform,
        bool dragging)
    {
        // For dragging, place footprint at center
        if (dragging)
            return new EntityCoordinates(gridUid, transform.LocalPosition);

        // For walking, offset left or right based on which foot
        var offset = component.RightStep
            ? new Angle(Angle.FromDegrees(180f) + transform.LocalRotation).RotateVec(component.OffsetPrint)
            : new Angle(transform.LocalRotation).RotateVec(component.OffsetPrint);

        return new EntityCoordinates(gridUid, transform.LocalPosition + offset);
    }

    private FootPrintVisuals PickState(Entity<FootPrintsComponent> ent, bool dragging)
    {
        // If dragging, always use drag marks
        if (dragging)
            return FootPrintVisuals.Dragging;

        // Check for shoes
        if (_inventory.TryGetSlotEntity(ent, "shoes", out _))
            return FootPrintVisuals.ShoesPrint;

        // Check for hardsuit
        if (_inventory.TryGetSlotEntity(ent, "outerClothing", out var suit) && _tag.HasTag(suit.Value, HardsuitTag))
            return FootPrintVisuals.SuitPrint;

        // Default to bare feet
        return FootPrintVisuals.BareFootPrint;
    }
}
