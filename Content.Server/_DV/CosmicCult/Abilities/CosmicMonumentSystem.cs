using System.Numerics;
using Content.Server._DV.CosmicCult.Components;
using Content.Server._DV.CosmicCult.EntitySystems;
using Content.Server.Actions;
using Content.Server.Popups;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicMonumentSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly CosmicCorruptingSystem _corrupting = default!;
    [Dependency] private readonly CosmicCultRuleSystem _cultRule = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly StationSystem _station = default!;

    private EntityUid? _monumentStorageMap;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCultLeadComponent, EventCosmicPlaceMonument>(OnCosmicPlaceMonument);
        SubscribeLocalEvent<CosmicCultLeadComponent, EventCosmicMoveMonument>(OnCosmicMoveMonument);
    }

    //todo attack this with a debugger at some point, it seems to un-prime before it should sometimes?
    //no idea why, might be something to do with verifying placement inside the action's execution instead of in an attemptEvent beforehand?
    //yeah it is - if the action is primed but fails at this step, then the action becomes un-primed but does not properly go through, requiring it to be primed again
    //works fine:tm: for now with a slightly jank fix on the client end of things, will probably want to dig deeper?
    //actually might not want to fix it?
    //I've got the client stuff working well & this works out to making the ghost stay up so long as you consistently try (& fail) to place the monument
    //guess I should ask for specific feedback for this one tiny feature?
    private void OnCosmicPlaceMonument(Entity<CosmicCultLeadComponent> uid, ref EventCosmicPlaceMonument args)
    {
        if (!VerifyPlacement(uid, out var pos))
            return;

        _actions.RemoveAction(uid, uid.Comp.CosmicMonumentPlaceActionEntity);

        Spawn("MonumentCollider", pos);
        var monument = Spawn(uid.Comp.MonumentPrototype, pos);

        _cultRule.TransferCultAssociation(uid, monument);
    }

    private void OnCosmicMoveMonument(Entity<CosmicCultLeadComponent> uid, ref EventCosmicMoveMonument args)
    {
        if (_cultRule.AssociatedGamerule(uid) is not {} cult)
            return;

        if (!VerifyPlacement(uid, out var pos))
            return;

        _actions.RemoveAction(uid, uid.Comp.CosmicMonumentMoveActionEntity);

        //delete all old monument colliders for 100% safety
        var colliderQuery = EntityQueryEnumerator<MonumentCollisionComponent>();
        while (colliderQuery.MoveNext(out var collider, out _))
        {
            QueueDel(collider);
        }

        //spawn the destination effect first because we only need one
        var destEnt = Spawn("MonumentCosmicCultMoveEnd", pos);
        var destComp = EnsureComp<MonumentMoveDestinationComponent>(destEnt);
        var coords = Transform(cult.Comp.MonumentInGame).Coordinates;
        Spawn("MonumentCollider", pos); //spawn a new collider

        Spawn("MonumentCosmicCultMoveStart", coords);
        Spawn("MonumentCollider", Transform(cult.Comp.MonumentInGame).Coordinates); //spawn a new collider

        //timers!
        //move the monument to cheese world (the storage map)
        //these timers aren't amazing because they're basically magic values but eh
        Timer.Spawn(TimeSpan.FromSeconds(0.45),
            () =>
            {
                //todo check if anything gets messed up by doing this to the monument?
                _transform.SetParent(cult.Comp.MonumentInGame, EnsureStorageMapExists());
                destComp.Monument = cult.Comp.MonumentInGame; //only get the first monument

                if (cult.Comp.MonumentInGame.Comp.CurrentGlyph is not null) //delete the scribed glyph as well
                    QueueDel(cult.Comp.MonumentInGame.Comp.CurrentGlyph);

                //close the UI for everyone who has it open
                _ui.CloseUi(uid.Owner, MonumentKey.Key);

                //retrieve the monument from cheese world
                Timer.Spawn(TimeSpan.FromSeconds(0.75), //relative to the last timer
                    () =>
                    {
                        var colliderQuery = EntityQueryEnumerator<MonumentCollisionComponent>();
                        while (colliderQuery.MoveNext(out var collider, out _))
                        {
                            QueueDel(collider);
                        }

                        if (destComp.Monument is null)
                            return;

                        var xform = Transform(destEnt);
                        _transform.SetCoordinates(destComp.Monument.Value, xform.Coordinates);
                        _transform.AnchorEntity(destComp.Monument.Value); //no idea if this does anything but let's be safe about it
                        Spawn("MonumentCollider", xform.Coordinates);

                        if (TryComp<CosmicCorruptingComponent>(destComp.Monument.Value, out var cosmicCorruptingComp))
                            _corrupting.RecalculateStartingTiles((destComp.Monument.Value, cosmicCorruptingComp));
                    });
            });
    }

    private EntityUid EnsureStorageMapExists()
    {
        if (_monumentStorageMap != null && Exists(_monumentStorageMap))
            return _monumentStorageMap.Value;

        _monumentStorageMap = _map.CreateMap();
        _map.SetPaused(_monumentStorageMap.Value, true);
        return _monumentStorageMap.Value;
    }

    //todo this can probably be mostly moved to shared but my brain isn't cooperating w/ that rn
    private bool VerifyPlacement(Entity<CosmicCultLeadComponent> uid, out EntityCoordinates outPos)
    {
        //MAKE SURE WE'RE STANDING ON A GRID
        var xform = Transform(uid);
        outPos = new EntityCoordinates();

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-monument-spawn-error-grid"), uid, uid);
            return false;
        }

        var localTile = _map.GetTileRef(xform.GridUid.Value, grid, xform.Coordinates);
        var targetIndices = localTile.GridIndices + new Vector2i(0, 1);
        var pos = _map.ToCenterCoordinates(xform.GridUid.Value, targetIndices, grid);
        outPos = pos;
        var box = new Box2(pos.Position + new Vector2(-1.4f, -0.4f), pos.Position + new Vector2(1.4f, 0.4f));

        //CHECK IF IT'S BEING PLACED CHEESILY CLOSE TO SPACE
        var spaceDistance = 3;
        var worldPos = _transform.GetWorldPosition(xform);
        foreach (var tile in _map.GetTilesIntersecting(xform.GridUid.Value, grid, new Circle(worldPos, spaceDistance)))
        {
            if (tile.IsSpace(_tileDef))
            {
                _popup.PopupEntity(Loc.GetString("cosmicability-monument-spawn-error-space", ("DISTANCE", spaceDistance)), uid, uid);
                return false;
            }
        }

        //CHECK IF WE'RE ON THE STATION OR IF SOMEONE'S TRYING TO SNEAK THIS ONTO SOMETHING SMOL
        var station = _station.GetStationInMap(xform.MapID);

        EntityUid? stationGrid = null;

        if (TryComp<StationDataComponent>(station, out var stationData))
            stationGrid = _station.GetLargestGrid(stationData);

        if (stationGrid is not null && stationGrid != xform.GridUid)
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-monument-spawn-error-station"), uid, uid);
            return false;
        }

        //CHECK FOR ENTITY AND ENVIRONMENTAL INTERSECTIONS
        if (_lookup.AnyLocalEntitiesIntersecting(xform.GridUid.Value, box, LookupFlags.Dynamic | LookupFlags.Static, uid))
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-monument-spawn-error-intersection"), uid, uid);
            return false;
        }

        return true;
    }
}
