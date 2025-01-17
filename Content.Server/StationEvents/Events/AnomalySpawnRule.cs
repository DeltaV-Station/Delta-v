using Content.Server.Anomaly;
using Content.Server.Station.Components;
using Content.Server.StationEvents.Components;
﻿using Content.Shared.GameTicking.Components;
using Content.Server._EE.Announcements.Systems; // Impstation Random Announcer System
using Robust.Shared.Player; // Impstation Random Announcer System

namespace Content.Server.StationEvents.Events;

public sealed class AnomalySpawnRule : StationEventSystem<AnomalySpawnRuleComponent>
{
    [Dependency] private readonly AnomalySystem _anomaly = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!; // Impstation Random Announcer System

    protected override void Added(EntityUid uid, AnomalySpawnRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        _announcer.SendAnnouncement( // Start Impstation Random Announcer System: Integrates the announcer
            _announcer.GetAnnouncementId(args.RuleId),
            Filter.Broadcast(),
            "anomaly-spawn-event-announcement",
            null,
            Color.FromHex("#18abf5"),
            null, null,
            ("sighting", Loc.GetString($"anomaly-spawn-sighting-{RobustRandom.Next(1, 6)}"))
        ); // End Impstation Random Announcer System

        base.Added(uid, component, gameRule, args);
    }

    protected override void Started(EntityUid uid, AnomalySpawnRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

        if (!TryComp<StationDataComponent>(chosenStation, out var stationData))
            return;

        var grid = StationSystem.GetLargestGrid(stationData);

        if (grid is null)
            return;

        var amountToSpawn = 1;
        for (var i = 0; i < amountToSpawn; i++)
        {
            _anomaly.SpawnOnRandomGridLocation(grid.Value, component.AnomalySpawnerPrototype);
        }
    }
}
