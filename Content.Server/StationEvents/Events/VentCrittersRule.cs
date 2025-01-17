using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Pinpointer;
using Content.Server.StationEvents.Components;
using Content.Shared.EntityTable;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using Content.Shared.Storage;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Content.Server._EE.Announcements.Systems; // Impstation Random Announcer System
using Robust.Shared.Player; // Impstation Random Announcer System

namespace Content.Server.StationEvents.Events;

/// <summary>
/// DeltaV: Reworked vent critters to spawn a number of mobs at a single telegraphed location.
/// This gives players time to run away and let sec do their job.
/// </summary>
/// <remarks>
/// This entire file is rewritten, ignore upstream changes.
/// </remarks>
public sealed class VentCrittersRule : StationEventSystem<VentCrittersRuleComponent>
{
    /*
     * DO NOT COPY PASTE THIS TO MAKE YOUR MOB EVENT.
     * USE THE PROTOTYPE.
     */

    [Dependency] private readonly AnnouncerSystem _announcer = default!; // Impstation Random Announcer System
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private List<EntityCoordinates> _locations = new();

    protected override void Added(EntityUid uid, VentCrittersRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args) // Begin Impstation Random Announcer System
    {
        base.Added(uid, component, gameRule, args);

        PickLocation(component);
        if (component.Location is not { } coords) // End Impstation Random Announcer System
        {
            ForceEndSelf(uid, gameRule);
            return;
        }

        var mapCoords = _transform.ToMapCoordinates(coords);
        if (!_navMap.TryGetNearestBeacon(mapCoords, out var beacon, out _))
            return;

        var nearest = beacon?.Comp?.Text!;

        _announcer.SendAnnouncement( // Begin Impstation Random Announcer System: Integrates the announcer
            _announcer.GetAnnouncementId(args.RuleId),
            Filter.Broadcast(),
            "station-event-vent-creatures-start-announcement-deltav",
            null,
            Color.Gold,
            null, null,
            ("location", nearest)
        ); // End Impstation Random Announcer System
    }

    protected override void Ended(EntityUid uid, VentCrittersRuleComponent comp, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, comp, gameRule, args);

        if (comp.Location is not { } coords)
            return;

        var players = _antag.GetTotalPlayerCount(_player.Sessions);
        var min = Math.Max(comp.Min, comp.Min * players / comp.PlayerRatio);
        var max = Math.Max(comp.Max, comp.Max * players / comp.PlayerRatio);
        var count = Math.Max(RobustRandom.Next(min, max), 1);
        Log.Info($"Spawning {count} critters for {ToPrettyString(uid):rule}");

        for (int i = 0; i < count; i++)
        {
            foreach (var spawn in _entityTable.GetSpawns(comp.Table))
            {
                Spawn(spawn, coords);
            }
        }

        if (comp.SpecialEntries.Count == 0)
            return;

        // guaranteed spawn
        var specialEntry = RobustRandom.Pick(comp.SpecialEntries);
        Spawn(specialEntry.PrototypeId, coords);
    }

    private void PickLocation(VentCrittersRuleComponent component)
    {
        if (!TryGetRandomStation(out var station))
            return;

        var locations = EntityQueryEnumerator<VentCritterSpawnLocationComponent, TransformComponent>();
        _locations.Clear();
        while (locations.MoveNext(out var uid, out _, out var transform))
        {
            if (CompOrNull<StationMemberComponent>(transform.GridUid)?.Station == station)
            {
                _locations.Add(transform.Coordinates);
            }
        }

        if (_locations.Count > 0)
            component.Location = RobustRandom.Pick(_locations);
    }
}
