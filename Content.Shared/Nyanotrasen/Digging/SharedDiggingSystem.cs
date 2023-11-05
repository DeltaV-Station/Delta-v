using System.Threading;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Nyanotrasen.Digging;
using Content.Shared.Physics;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared.Digging;

public sealed class SharedDiggingSystem : EntitySystem
{
    [Dependency] private  readonly TileSystem _tiles = default!;
    [Dependency] private  readonly SharedMapSystem _maps = default!;
    [Dependency] private  readonly SharedToolSystem _tools = default!;
    [Dependency] private  readonly TurfSystem _turfs = default!;
    [Dependency] private  readonly IMapManager _mapManager = default!;
    [Dependency] private  readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private  readonly SharedInteractionSystem _interactionSystem = default!;
    // [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EarthDiggingComponent, AfterInteractEvent>(OnDiggingAfterInteract);
        SubscribeLocalEvent<EarthDiggingComponent, EarthDiggingCompleteEvent>(OnEarthDigComplete);
    }



    private void OnEarthDigComplete(EntityUid shovel, EarthDiggingComponent comp, EarthDiggingCompleteEvent args)
    {
        var coordinates = GetCoordinates(args.Coordinates);
        if (!TryComp<EarthDiggingComponent>(shovel, out var component))
            return;


        var gridUid = coordinates.GetGridUid(EntityManager);
        if (gridUid == null)
            return;

        var grid = Comp<MapGridComponent>(gridUid.Value);
        var tile = _maps.GetTileRef(gridUid.Value, grid, coordinates);

        if (_tileDefManager[tile.Tile.TypeId] is not ContentTileDefinition tileDef
            || !tileDef.CanShovel
            //  || tileDef.BaseTurfs.Count == 0
            || _turfs.IsTileBlocked(tile, CollisionGroup.MobMask))
        {
            return;
        }

        _tiles.DigTile(tile);
    }

    private void OnDiggingAfterInteract(EntityUid uid, EarthDiggingComponent component,
        AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target != null)
            return;

        if (TryDig(args.User, uid, component, args.ClickLocation))
            args.Handled = true;
    }

    private bool TryDig(EntityUid user, EntityUid shovel, EarthDiggingComponent component,
        EntityCoordinates clickLocation)
    {
        ToolComponent? tool = null;
        if (component.ToolComponentNeeded && !TryComp(shovel, out tool))
            return false;

        if (!_mapManager.TryGetGrid(clickLocation.GetGridUid(EntityManager), out var mapGrid))
            return false;

        var tile = mapGrid.GetTileRef(clickLocation);

        var coordinates = mapGrid.GridTileToLocal(tile.GridIndices);

        if (!_interactionSystem.InRangeUnobstructed(user, coordinates, popup: false))
            return false;

        if (_tileDefManager[tile.Tile.TypeId] is not ContentTileDefinition tileDef
            || !tileDef.CanShovel
            || tileDef.BaseTurf.Length == 0
            // || _tileDefinitionManager[tileDef.BaseTurfs[^1]] is not ContentTileDefinition newDef
            || tile.IsBlockedTurf(true))
        {
            return false;
        }

        return _tools.UseTool(
            shovel,
            user,
            // FIXME
            target: shovel,
            doAfterDelay: component.Delay,
            toolQualitiesNeeded: new[] { component.QualityNeeded },
            doAfterEv: new EarthDiggingCompleteEvent
            {
                Coordinates = GetNetCoordinates(clickLocation),
                Shovel = GetNetEntity(shovel)
            },
            toolComponent: tool
        );
    }

}



