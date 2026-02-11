using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Gravity;
using Content.Shared._starcup.CCVars;
using Content.Shared._starcup.Footprints;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.Inventory;
using Content.Shared.Standing;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._starcup.Footprints;

public sealed class FootprintSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly GravitySystem _gravity = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private EntityQuery<FootprintModifierComponent> _footprintModiferQuery;

    /// <summary>
    /// How large of a volume a tile can hold before spilling into a puddle.
    /// Should match the puddle component's overflow size.
    /// </summary>
    public static readonly FixedPoint2 MaxFootprintVolumeOnTile = 50;

    private FixedPoint2 _minimumPuddleSize = 10;

    public override void Initialize()
    {
        SubscribeLocalEvent<FootprintComponent, FootprintCleanEvent>(OnFootprintClean);
        SubscribeLocalEvent<PuddleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FootprintOwnerComponent, MoveEvent>(OnMove);

        _footprintModiferQuery = GetEntityQuery<FootprintModifierComponent>();

        Subs.CVar(_configuration, SCCVars.MinimumPuddleSizeForFootprints, value => _minimumPuddleSize = value, true);
    }

    /// <summary>
    /// Footprints that get cleaned, either by absorbing or reactions, spill to standard puddles
    /// This prevents the spill to tile reaction from leaving behind an empty footprint
    /// </summary>
    private void OnFootprintClean(Entity<FootprintComponent> ent, ref FootprintCleanEvent args)
    {
        ToPuddle(ent);
    }

    /// <summary>
    /// Puddles that spawn on a tile with footprints on it absorb the footprint solutions
    /// </summary>
    private void OnMapInit(Entity<PuddleComponent> ent, ref MapInitEvent args)
    {
        if (HasComp<FootprintComponent>(ent))
            return;

        var transform = Transform(ent);

        if (transform.GridUid is not { } gridUid)
            return;

        if (!TryComp<MapGridComponent>(gridUid, out var gridComponent))
            return;

        var tile = _map.CoordinatesToTile(gridUid, gridComponent, transform.Coordinates);

        if (!TryGetAnchoredEntity<FootprintComponent>((gridUid, gridComponent), tile, out var footprint))
            return;

        ToPuddle(footprint.Value, transform.Coordinates);
    }

    /// <summary>
    /// Spills a <paramref name="footprint"/>'s solution into a puddle at <paramref name="coordinates"/>
    /// </summary>
    private void ToPuddle(Entity<FootprintComponent> footprint, EntityCoordinates? coordinates = null)
    {
        coordinates ??= Transform(footprint).Coordinates;

        if (!_solution.TryGetSolution(footprint.Owner, footprint.Comp.Solution, out _, out var footprintSolution))
            return;

        footprintSolution = footprintSolution.Clone();

        Del(footprint);

        _puddle.TrySpillAt(coordinates.Value, footprintSolution, out _, false);
    }

    private void OnMove(Entity<FootprintOwnerComponent> ent, ref MoveEvent args)
    {
        if (_gravity.IsWeightless(ent.AsType()) || !args.OldPosition.IsValid(EntityManager) || !args.NewPosition.IsValid(EntityManager))
            return;

        if (!_prototype.TryIndex(GetFootprintProto(ent), out var footprintData))
            return;

        var oldPosition = _transform.ToMapCoordinates(args.OldPosition).Position;
        var newPosition = _transform.ToMapCoordinates(args.NewPosition).Position;

        ent.Comp.Distance += Vector2.Distance(newPosition, oldPosition);

        if (ent.Comp.Distance < footprintData.Distance)
            return;

        ent.Comp.Distance -= footprintData.Distance;

        var transform = Transform(ent);

        if (transform.GridUid is not { } grid)
            return;

        if (!TryComp<MapGridComponent>(grid, out var gridComponent))
            return;

        ent.Comp.LastLayer = footprintData.Prints.GetLayerIndex(ent.Comp.LastLayer, _random);
        var layer = footprintData.Prints.Layers[ent.Comp.LastLayer];

        EntityCoordinates coordinates = new(ent, layer.Offset, 0);

        var tile = _map.CoordinatesToTile(grid, gridComponent, coordinates);

        if (TryPuddleInteraction(ent, (grid, gridComponent), tile, footprintData))
            return;

        Angle rotation;

        if (footprintData.LocalRotation)
            rotation = transform.LocalRotation;
        else
        {
            // Face the direction of movement instead
            var oldLocalPosition = _map.WorldToLocal(grid, gridComponent, oldPosition);
            var newLocalPosition = _map.WorldToLocal(grid, gridComponent, newPosition);

            rotation = Angle.FromWorldVec(newLocalPosition - oldLocalPosition);
        }

        FootprintInteraction(ent, (grid, gridComponent), tile, coordinates, rotation, footprintData);
    }

    /// <summary>
    /// Gets the most relavent footprint prototype for the entity
    /// </summary>
    private ProtoId<FootprintPrototype> GetFootprintProto(Entity<FootprintOwnerComponent> ent)
    {
        if (_standing.IsDown(ent.AsType()))
            return ent.Comp.Bodyprint;

        if (_inventory.TryGetSlotEntity(ent, "outerClothing", out var suit)
                && _footprintModiferQuery.TryComp(suit, out var suitMod)
                && suitMod.Footprint is { } suitModPrint)
            return suitModPrint;

        if (_inventory.TryGetSlotEntity(ent, "shoes", out var shoes)
                && _footprintModiferQuery.TryComp(shoes, out var shoeMod)
                && shoeMod.Footprint is { } shoeModPrint)
            return shoeModPrint;

        if (_footprintModiferQuery.TryComp(ent, out var entMod) && entMod.Footprint is { } entModPrint)
            return entModPrint;

        return ent.Comp.Footprint;
    }

    /// <summary>
    /// Attempt to transfer solutions between the footprint owner and any non-footprint puddles on the tile
    /// <return> true if the owner picked up some solution from a puddle on the tile
    /// </summary>
    private bool TryPuddleInteraction(Entity<FootprintOwnerComponent> ent, Entity<MapGridComponent> grid, Vector2i tile, FootprintPrototype footprintData)
    {
        if (!_puddle.TryGetPuddle(_map.GetTileRef(grid, tile), out var puddleUid))
            return false;

        var puddleComponent = Comp<PuddleComponent>(puddleUid);
        var puddleSolutionName = puddleComponent.SolutionName;

        if (!_solution.TryGetSolution(puddleUid, puddleSolutionName, out var puddleSolution, out var puddleSol))
            return false;

        if (!_solution.EnsureSolutionEntity(ent.Owner, ent.Comp.Solution, out _, out var footprintOwnerSolution, footprintData.MaxStoredVolume))
            return false;

        var footprintOwnerSol = footprintOwnerSolution.Value.Comp.Solution;

        _solution.TryTransferSolution(puddleSolution.Value, source: footprintOwnerSol, GetScaledVolume(footprintOwnerSol, footprintData));

        // Only pick up volume from puddles that are big enough
        if (puddleSol.Volume < _minimumPuddleSize)
            return false;

        _solution.TryTransferSolution(footprintOwnerSolution.Value, source: puddleSol, FixedPoint2.Max(0, footprintData.MaxStoredVolume - footprintOwnerSol.Volume));
        _solution.UpdateChemicals(puddleSolution.Value, false);

        return true;
    }

    /// <summary>
    /// Attempt to lay down a footprint on the tile
    /// </summary>
    private void FootprintInteraction(Entity<FootprintOwnerComponent> ent, Entity<MapGridComponent> grid, Vector2i tile, EntityCoordinates coordinates, Angle rotation, FootprintPrototype footprintData)
    {
        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out var solution, out _))
            return;

        var ownerSolution = solution.Value.Comp.Solution;
        var volume = GetScaledVolume(ownerSolution, footprintData);

        // Get or spawn a footprint entity
        if (!TryGetAnchoredEntity<FootprintComponent>(grid, tile, out var fp))
        {
            var fpEntity = SpawnAtPosition(ent.Comp.FootprintEntityPrototype, coordinates);
            fp = (fpEntity, Comp<FootprintComponent>(fpEntity));
        }
        var footprint = fp.Value;

        if (!_solution.EnsureSolutionEntity(footprint.Owner, footprint.Comp.Solution, out _, out var footprintSolution, MaxFootprintVolumeOnTile))
            return;

        _solution.TryTransferSolution(footprintSolution.Value, source: ownerSolution, volume);

        // Ensure no trace amounts are left after the last footprint is put down
        if (volume < footprintData.MinVolume)
            _solution.RemoveAllSolution(solution.Value);

        // Too many footprints, become a normal puddle
        if (footprintSolution.Value.Comp.Solution.Volume >= MaxFootprintVolumeOnTile)
        {
            ToPuddle(footprint, coordinates);
            return;
        }

        var tileOffset = CalcTileOffset(grid, coordinates);
        var alpha = (float)volume / footprintData.MaxVolume / 2f;
        footprint.Comp.Footprints.Add(new(tileOffset, rotation, alpha, footprintData.Prints.Layers[ent.Comp.LastLayer].State ?? "footprint-shoes"));

        Dirty(footprint);

        if (TryGetNetEntity(footprint, out var netFootprint))
            RaiseNetworkEvent(new FootprintChangedEvent(netFootprint.Value), Filter.Pvs(footprint));
    }

    /// <summary>
    /// Compute the position that the footprint should be in relative to its parent tile
    /// </summary>
    private Vector2 CalcTileOffset(Entity<MapGridComponent> grid, EntityCoordinates coordinates)
    {
        var gridCoords = _map.LocalToGrid(grid, grid, coordinates);
        var tileSize = grid.Comp.TileSize;

        var x = gridCoords.X / tileSize;
        var y = gridCoords.Y / tileSize;

        var halfTileSize = tileSize / 2f;

        x -= MathF.Floor(x) + halfTileSize;
        y -= MathF.Floor(y) + halfTileSize;

        return new(x, y);
    }

    private static FixedPoint2 GetScaledVolume(Solution solution, FootprintPrototype footprint)
    {
        return GetScaledVolume(solution.Volume, footprint.MaxStoredVolume, footprint.MinVolume, footprint.MaxVolume);
    }

    private static FixedPoint2 GetScaledVolume(FixedPoint2 valueA, FixedPoint2 maxA, FixedPoint2 minB, FixedPoint2 maxB)
    {
        var ratio = valueA / maxA;
        var scale = maxB - minB;
        var amount = ratio * scale + minB;
        return FixedPoint2.Min(valueA, amount);
    }

    private bool TryGetAnchoredEntity<T>(Entity<MapGridComponent> grid, Vector2i pos, [NotNullWhen(true)] out Entity<T>? entity) where T : IComponent
    {
        var anchoredEnumerator = _map.GetAnchoredEntitiesEnumerator(grid, grid, pos);
        var entityQuery = GetEntityQuery<T>();

        entity = null;
        while (anchoredEnumerator.MoveNext(out var ent))
        {
            if (entityQuery.TryComp(ent, out var comp))
            {
                entity = (ent.Value, comp);
                break;
            }
        }
        return entity != null;
    }
}
