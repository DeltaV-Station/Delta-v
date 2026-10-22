using Content.Server.Antag;
using Content.Server.StationEvents.Components;
using Robust.Shared.Map;

namespace Content.Server.StationEvents.Events;

/// <summary>
/// Makes antags spawn at a random midround antag or vent critter spawner.
/// </summary>
public sealed class MidRoundAntagRule : StationEventSystem<MidRoundAntagRuleComponent>
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MidRoundAntagRuleComponent, AntagSelectLocationEvent>(OnSelectLocation);
    }

    private void OnSelectLocation(Entity<MidRoundAntagRuleComponent> ent, ref AntagSelectLocationEvent args)
    {
        if (!TryGetRandomStation(out var station))
            return;

        var spawns = FindSpawns(ent, station.Value);
        if (spawns.Count == 0)
        {
            Log.Warning($"Couldn't find any suitable midround antag spawners for {ToPrettyString(ent):rule}");
            return;
        }

        args.Coordinates.AddRange(spawns);
    }

    private void FindSpawnLocations(EntityUid station, List<MapCoordinates> spawns)
    {
        var query = EntityQueryEnumerator<MidRoundAntagSpawnLocationComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (StationSystem.GetOwningStation(uid, xform) == station && xform.GridUid != null)
                spawns.Add(_xform.GetMapCoordinates(xform));
        }
    }

    private void FindVentLocations(EntityUid station, List<MapCoordinates> spawns)
    {
        var fallbackQuery = EntityQueryEnumerator<VentCritterSpawnLocationComponent, TransformComponent>();
        while (fallbackQuery.MoveNext(out var uid, out _, out var xform))
        {
            if (StationSystem.GetOwningStation(uid, xform) == station && xform.GridUid != null)
                spawns.Add(_xform.GetMapCoordinates(xform));
        }
    }

    private List<MapCoordinates> FindSpawns(Entity<MidRoundAntagRuleComponent> rule, EntityUid station)
    {
        var spawns = new List<MapCoordinates>();

        if (rule.Comp.PreferVentSpawns)
            FindVentLocations(station, spawns);
        else
            FindSpawnLocations(station, spawns);

        // if there are any midround antag spawns mapped, use them
        if (spawns.Count > 0)
            return spawns;

        if (rule.Comp.PreferVentSpawns)
            Log.Info($"Station {ToPrettyString(station):station} has no vent critter spawnpoints mapped, falling back. Please map them!");
        else
            Log.Info($"Station {ToPrettyString(station):station} has no midround antag spawnpoints mapped, falling back. Please map them!");

        if (rule.Comp.PreferVentSpawns)
            FindSpawnLocations(station, spawns);
        else
            FindVentLocations(station, spawns);

        return spawns;
    }
}
