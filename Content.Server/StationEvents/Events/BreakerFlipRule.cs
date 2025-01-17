using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using JetBrains.Annotations;
using Content.Server._EE.Announcements.Systems; // Impstation Random Announcer System
using Robust.Shared.Player; // Impstation Random Announcer System

namespace Content.Server.StationEvents.Events;

[UsedImplicitly]
public sealed class BreakerFlipRule : StationEventSystem<BreakerFlipRuleComponent>
{
    [Dependency] private readonly ApcSystem _apcSystem = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!; // Impstation Random Announcer System

    protected override void Added(EntityUid uid, BreakerFlipRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        base.Added(uid, component, gameRule, args);

        _announcer.SendAnnouncement( // Start Impstation Random Announcer System: Integrates the announcer
            _announcer.GetAnnouncementId(args.RuleId),
            Filter.Broadcast(),
            "station-event-breaker-flip-announcement",
            null,
            Color.Gold,
            null, null,
            ("data", Loc.GetString($"random-sentience-event-data-{RobustRandom.Next(1, 6)}"))
        ); // End Impstation Random Announcer System
    }

    protected override void Started(EntityUid uid, BreakerFlipRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

        var stationApcs = new List<Entity<ApcComponent>>();
        var query = EntityQueryEnumerator<ApcComponent, TransformComponent>();
        while (query.MoveNext(out var apcUid, out var apc, out var xform))
        {
            if (apc.MainBreakerEnabled && CompOrNull<StationMemberComponent>(xform.GridUid)?.Station == chosenStation)
            {
                stationApcs.Add((apcUid, apc));
            }
        }

        var toDisable = Math.Min(RobustRandom.Next(3, 7), stationApcs.Count);
        if (toDisable == 0)
            return;

        RobustRandom.Shuffle(stationApcs);

        for (var i = 0; i < toDisable; i++)
        {
            _apcSystem.ApcToggleBreaker(stationApcs[i], stationApcs[i]);
        }
    }
}
