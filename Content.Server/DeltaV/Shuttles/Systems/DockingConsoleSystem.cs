using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.DeltaV.Shuttles;
using Content.Shared.DeltaV.Shuttles.Components;
using Content.Shared.DeltaV.Shuttles.Systems;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.Timing;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.DeltaV.Shuttles.Systems;

public sealed class DockingConsoleSystem : SharedDockingConsoleSystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DockEvent>(OnDock);
        SubscribeLocalEvent<UndockEvent>(OnUndock);

        Subs.BuiEvents<DockingConsoleComponent>(DockingConsoleUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnOpened);
            subs.Event<DockingConsoleFTLMessage>(OnFTL);
        });
    }

    private void OnDock(DockEvent args)
    {
        UpdateConsoles(args.GridAUid, args.GridBUid);
    }

    private void OnUndock(UndockEvent args)
    {
        UpdateConsoles(args.GridAUid, args.GridBUid);
    }

    private void OnOpened(Entity<DockingConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        if (TerminatingOrDeleted(ent.Comp.Shuttle))
            UpdateShuttle(ent);

        UpdateUI(ent);
    }

    private void UpdateConsoles(EntityUid gridA, EntityUid gridB)
    {
        UpdateConsolesUsing(gridA);
        UpdateConsolesUsing(gridB);
    }

    /// <summary>
    /// Update the UI of every console that is using a certain shuttle.
    /// </summary>
    public void UpdateConsolesUsing(EntityUid shuttle)
    {
        if (!HasComp<DockingShuttleComponent>(shuttle))
            return;

        var query = EntityQueryEnumerator<DockingConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Shuttle == shuttle)
                UpdateUI((uid, comp));
        }
    }

    private void UpdateUI(Entity<DockingConsoleComponent> ent)
    {
        if (ent.Comp.Shuttle is not {} shuttle)
            return;

        var ftlState = FTLState.Available;
        StartEndTime ftlTime = default;
        List<DockingDestination> destinations = new();

        if (TryComp<FTLComponent>(shuttle, out var ftl))
        {
            ftlState = ftl.State;
            ftlTime = _shuttle.GetStateTime(ftl);
        }

        if (TryComp<DockingShuttleComponent>(shuttle, out var docking))
        {
            destinations = docking.Destinations;
        }

        var state = new DockingConsoleState(ftlState, ftlTime, destinations);
        _ui.SetUiState(ent.Owner, DockingConsoleUiKey.Key, state);
    }

    private void OnFTL(Entity<DockingConsoleComponent> ent, ref DockingConsoleFTLMessage args)
    {
        if (ent.Comp.Shuttle is not {} shuttle || !TryComp<DockingShuttleComponent>(shuttle, out var docking))
            return;

        if (args.Index < 0 || args.Index > docking.Destinations.Count)
            return;

        var dest = docking.Destinations[args.Index];
        var map = dest.Map;
        // can't FTL if its already there or somehow failed whitelist
        if (map == Transform(shuttle).MapID || !_shuttle.CanFTLTo(shuttle, map, ent))
            return;

        if (FindLargestGrid(map) is not {} grid)
            return;

        Log.Debug($"{ToPrettyString(args.Actor):user} is FTL-docking {ToPrettyString(shuttle):shuttle} to {ToPrettyString(grid):grid}");

        _shuttle.FTLToDock(shuttle, Comp<ShuttleComponent>(shuttle), grid, priorityTag: ent.Comp.DockTag);
    }

    private EntityUid? FindLargestGrid(MapId map)
    {
        EntityUid? largestGrid = null;
        var largestSize = 0f;

        if (_station.GetStationInMap(map) is {} station)
        {
            // prevent picking vgroid and stuff
            return _station.GetLargestGrid(Comp<StationDataComponent>(station));
        }

        var query = EntityQueryEnumerator<MapGridComponent, TransformComponent>();
        while (query.MoveNext(out var gridUid, out var grid, out var xform))
        {
            if (xform.MapID != map)
                continue;

            var size = grid.LocalAABB.Size.LengthSquared();
            if (size < largestSize)
                continue;

            largestSize = size;
            largestGrid = gridUid;
        }

        return largestGrid;
    }

    private void UpdateShuttle(Entity<DockingConsoleComponent> ent)
    {
        var hadShuttle = ent.Comp.HasShuttle;
        // no error if it cant find one since it would fail every test as shuttle.grid_fill is false in dev
        ent.Comp.Shuttle = FindShuttle(ent.Comp.ShuttleWhitelist);
        ent.Comp.HasShuttle = ent.Comp.Shuttle != null;

        if (ent.Comp.HasShuttle != hadShuttle)
            Dirty(ent);
    }

    private EntityUid? FindShuttle(EntityWhitelist whitelist)
    {
        var query = EntityQueryEnumerator<DockingShuttleComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (_whitelist.IsValid(whitelist, uid))
                return uid;
        }

        return null;
    }
}
