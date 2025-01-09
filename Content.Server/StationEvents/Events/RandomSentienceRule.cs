using System.Linq;
using Content.Shared.Dataset;
using Content.Server.Ghost.Roles.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Server._EE.Announcements.Systems; // Impstation: RandomAnnouncerSystem Port from EE 
using Content.Server.Station.Components; // Impstation: RandomAnnouncerSystem Port from EE 

namespace Content.Server.StationEvents.Events;

public sealed class RandomSentienceRule : StationEventSystem<RandomSentienceRuleComponent>
{
    [Dependency] private readonly AnnouncerSystem _announcer = default!; // Impstation: RandomAnnouncerSystem Port from EE 

    protected override void Started(EntityUid uid, RandomSentienceRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        if (!TryGetRandomStation(out var station))
            return;

        var targetList = new List<Entity<SentienceTargetComponent>>();
        var query = EntityQueryEnumerator<SentienceTargetComponent, TransformComponent>();
        while (query.MoveNext(out var targetUid, out var target, out var xform))
        {
            if (StationSystem.GetOwningStation(targetUid, xform) != station)
                continue;

            targetList.Add((targetUid, target));
        }

        var toMakeSentient = _random.Next(component.MinSentiences, component.MaxSentiences);

        var groups = new HashSet<string>();

        for (var i = 0; i < toMakeSentient && targetList.Count > 0; i++)
        {
            // weighted random to pick a sentience target
            var totalWeight = targetList.Sum(x => x.Comp.Weight);
            // This initial target should never be picked.
            // It's just so that target doesn't need to be nullable and as a safety fallback for id floating point errors ever mess up the comparison in the foreach.
            var target = targetList[0];
            var chosenWeight = _random.NextFloat(totalWeight);
            var currentWeight = 0.0;
            foreach (var potentialTarget in targetList)
            {
                currentWeight += potentialTarget.Comp.Weight;
                if (currentWeight > chosenWeight)
                {
                    target = potentialTarget;
                    break;
                }
            }
            targetList.Remove(target);

            RemComp<SentienceTargetComponent>(target);
            var ghostRole = EnsureComp<GhostRoleComponent>(target);
            EnsureComp<GhostTakeoverAvailableComponent>(target);
            ghostRole.RoleName = MetaData(target).EntityName;
            ghostRole.RoleDescription = Loc.GetString("station-event-random-sentience-role-description", ("name", ghostRole.RoleName));
            groups.Add(Loc.GetString(target.Comp.FlavorKind));
        }

        if (groups.Count == 0)
            return;

        var groupList = groups.ToList();
        var kind1 = groupList.Count > 0 ? groupList[0] : "???";
        var kind2 = groupList.Count > 1 ? groupList[1] : "???";
        var kind3 = groupList.Count > 2 ? groupList[2] : "???";

        foreach (var target in targetList) //Impstation: Start RandomAnnouncerSystem Port from EE related code
        {
            var station = StationSystem.GetOwningStation(target);
            if(station == null)
                continue;
            stationsToNotify.Add((EntityUid) station);
        }
        foreach (var station in stationsToNotify)
        {
            _announcer.SendAnnouncement(
                _announcer.GetAnnouncementId(args.RuleId),
                StationSystem.GetInStation(EntityManager.GetComponent<StationDataComponent>(station)),
                "station-event-random-sentience-announcement",
                null,
                Color.Gold,
                null, null,
                ("kind1", kind1), ("kind2", kind2), ("kind3", kind3), ("amount", groupList.Count),
                    ("data", Loc.GetString($"random-sentience-event-data-{RobustRandom.Next(1, 6)}")),
                    ("strength", Loc.GetString($"random-sentience-event-strength-{RobustRandom.Next(1, 8)}"))
            );
        } // Impstation: End RandomAnnouncerSystem Port from EE related code
    }
}
