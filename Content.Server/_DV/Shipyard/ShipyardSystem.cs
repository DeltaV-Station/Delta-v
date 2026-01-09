using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._DV.CCVars;
using Content.Shared.Tag;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.Shipyard;

/// <summary>
/// Handles spawning and ftling ships.
/// </summary>
public sealed class ShipyardSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly MapDeleterShuttleSystem _mapDeleterShuttle = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;

    public ProtoId<TagPrototype> DockTag = "DockShipyard";

    public bool Enabled;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_config, DCCVars.Shipyard, value => Enabled = value, true);
    }

    /// <summary>
    /// Creates a ship from its yaml path in the shipyard.
    /// </summary>
    public bool TryCreateShuttle(ResPath path, [NotNullWhen(true)] out Entity<ShuttleComponent>? shuttle)
    {
        shuttle = null;
        if (!Enabled)
            return false;

        var map = _map.CreateMap(out var mapId);
        if (!_mapLoader.TryLoadGrid(mapId, path, out var grid))
        {
            Log.Error($"Failed to load shuttle {path}");
            Del(map);
            return false;
        }

        if (!TryComp<ShuttleComponent>(grid, out var comp))
        {
            Log.Error($"Shuttle {path}'s grid was missing ShuttleComponent");
            Del(map);
            return false;
        }

        _map.SetPaused(map, false);
        _mapDeleterShuttle.Enable(grid.Value);
        shuttle = (grid.Value, comp);
        return true;
    }

    /// <summary>
    /// Adds a ship to the shipyard and attempts to ftl-dock it to the given grid.
    /// </summary>
    public bool TrySendShuttle(Entity<ShuttleComponent?> shuttleDestination, ResPath path, [NotNullWhen(true)] out Entity<ShuttleComponent>? shuttle)
    {
        shuttle = null;
        if (!Resolve(shuttleDestination, ref shuttleDestination.Comp))
            return false;

        if (!TryCreateShuttle(path, out shuttle))
            return false;

        Log.Info($"Shuttle {path} was spawned for {ToPrettyString(shuttleDestination):station}");
        _shuttle.FTLToDock(shuttle.Value, shuttle.Value.Comp, shuttleDestination, priorityTag: DockTag);
        return true;
    }
}
