using System.Linq; // Impstation Random Announcer System
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using JetBrains.Annotations;
using Robust.Shared.Random;
using Content.Server._EE.Announcements.Systems; // Impstation Random Announcer System
using Robust.Shared.Player; // Impstation Random Announcer System

namespace Content.Server.StationEvents.Events;

[UsedImplicitly]
public sealed class FalseAlarmRule : StationEventSystem<FalseAlarmRuleComponent>
{
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!; // Impstation Random Announcer System

    protected override void Started(EntityUid uid, FalseAlarmRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        base.Started(uid, component, gameRule, args); // Start Impstation Random Announcer System: Integrating the announcer

        var allEv = _event.AllEvents()
            .Where(p => p.Value.StartAnnouncement)
            .Select(p => p.Key).ToList();
        var picked = RobustRandom.Pick(allEv);

        _announcer.SendAnnouncement(
            _announcer.GetAnnouncementId(picked.ID),
            Filter.Broadcast(),
            _announcer.GetEventLocaleString(_announcer.GetAnnouncementId(picked.ID)),
            null,
            Color.Gold,
            null, null,
            //TODO This isn't a good solution, but I can't think of something better
            ("data", Loc.GetString($"random-sentience-event-data-{RobustRandom.Next(1, 6)}"))
        ); // End Impstation Random Announcer System
    }
}
