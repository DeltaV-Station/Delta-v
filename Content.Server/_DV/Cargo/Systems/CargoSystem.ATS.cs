using Content.Server.Cargo.Components;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Systems;
using Content.Shared.Cargo.Components;
using Content.Shared.CCVar;
using Content.Shared.Shuttles.Components;
using Content.Shared.Tiles;
using Content.Shared.Whitelist;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server.Cargo.Systems;

/// <summary>
/// Loads the ATS to its own map that the cargo shuttle must FTL to.
/// </summary>
public sealed partial class CargoSystem
{
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    public EntityUid? CargoMap;

    private bool _gridFillEnabled;

    private void InitializeATS()
    {
        SubscribeLocalEvent<StationInitializedEvent>(OnStationInitialize); // DeltaV - trade station map

        Subs.CVar(_cfg, CCVars.GridFill, SetGridFill, true);
    }

    private void OnStationInitialize(StationInitializedEvent args)
    {
        if (!HasComp<StationCargoOrderDatabaseComponent>(args.Station)) // No cargo, L
            return;

        if (_gridFillEnabled)
            SetupTradePost();
    }

    private void SetGridFill(bool enabled)
    {
        _gridFillEnabled = enabled;
        if (enabled && _ticker.RunLevel != GameRunLevel.PreRoundLobby) // Ensure run level is in game
        {
            SetupTradePost();
        }
    }

    private void SetupTradePost()
    {
        if (Exists(CargoMap))
            return;

        var mapUid = _map.CreateMap(out var mapId);
        // Oh boy oh boy, hardcoded paths!
        var path = new ResPath("/Maps/Shuttles/trading_outpost.yml");
        if (!_mapLoader.TryLoadGrid(mapId, path, out var grid))
        {
            Log.Error($"Loading ATS from {path} failed!");
            Del(mapUid);
            return;
        }

        CargoMap = mapUid;

        var gridUid = grid.Value;
        EnsureComp<ProtectedGridComponent>(gridUid);
        EnsureComp<TradeStationComponent>(gridUid);

        var shuttleComp = EnsureComp<ShuttleComponent>(gridUid);
        shuttleComp.AngularDamping = 10000;
        shuttleComp.LinearDamping = 10000;

        var ftl = EnsureComp<FTLDestinationComponent>(mapUid);
        ftl.Whitelist = new EntityWhitelist()
        {
            Components =
            [
                _factory.GetComponentName(typeof(CargoShuttleComponent))
            ]
        };

        _metaSystem.SetEntityName(mapUid, $"Automated Trade Station {_random.Next(1000):000}");

        _console.RefreshShuttleConsoles();
    }

    private void CleanupTradeStation()
    {
        if (!Exists(CargoMap))
        {
            CargoMap = null;
            DebugTools.Assert(!EntityQuery<CargoShuttleComponent>().Any());
            return;
        }

        QueueDel(CargoMap);
        CargoMap = null;
    }
}
