using Content.Server.StationEvents.Components;
﻿using Content.Shared.GameTicking.Components;
using Robust.Shared.Random;
using Content.Server._EE.Announcements.Systems; // Impstation Random Announcer System
using Robust.Shared.Player; // Impstation Random Announcer System

namespace Content.Server.StationEvents.Events;

public sealed class BluespaceArtifactRule : StationEventSystem<BluespaceArtifactRuleComponent>
{
    [Dependency] private readonly AnnouncerSystem _announcer = default!; // Impstation Random Announcer System

    protected override void Added(EntityUid uid, BluespaceArtifactRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        base.Added(uid, component, gameRule, args);
        
        _announcer.SendAnnouncement( // Start Impstation Random Announcer System: Integrates the announcer
            _announcer.GetAnnouncementId(args.RuleId),
            Filter.Broadcast(),
            "bluespace-artifact-event-announcement",
            null,
            Color.FromHex("#18abf5"),
            null, null,
            ("sighting", Loc.GetString(RobustRandom.Pick(component.PossibleSighting)))
        ); // End Impstation Random Announcer System
    }

    protected override void Started(EntityUid uid, BluespaceArtifactRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var amountToSpawn = 1;
        for (var i = 0; i < amountToSpawn; i++)
        {
            if (!TryFindRandomTile(out _, out _, out _, out var coords))
                return;

            Spawn(component.ArtifactSpawnerPrototype, coords);
            Spawn(component.ArtifactFlashPrototype, coords);

            Sawmill.Info($"Spawning random artifact at {coords}");
        }
    }
}
