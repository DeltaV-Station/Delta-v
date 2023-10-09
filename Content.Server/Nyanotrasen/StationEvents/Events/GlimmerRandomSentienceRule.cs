using System.Linq;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Psionics;
using Content.Server.Speech.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Server.StationEvents.Events;

/// <summary>
/// Glimmer version of the (removed) random sentience event
/// </summary>
internal sealed class GlimmerRandomSentienceRule : StationEventSystem<GlimmerRandomSentienceRuleComponent>
{
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    protected override void Started(EntityUid uid, GlimmerRandomSentienceRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        List<EntityUid> targetList = new();

        var query = EntityQueryEnumerator<SentienceTargetComponent>();
        while (query.MoveNext(out var target, out _))
        {
            if (HasComp<GhostTakeoverAvailableComponent>(target))
                continue;

            if (!_mobStateSystem.IsAlive(target))
                continue;

            targetList.Add(target);
        }

        RobustRandom.Shuffle(targetList);

        var toMakeSentient = RobustRandom.Next(1, component.MaxMakeSentient);

        foreach (var target in targetList)
        {
            if (toMakeSentient-- == 0)
                break;

            EntityManager.RemoveComponent<SentienceTargetComponent>(target);
            MetaData(target).EntityName = Loc.GetString("glimmer-event-awakened-prefix", ("entity", target));
            var comp = EntityManager.EnsureComponent<GhostRoleComponent>(target);
            comp.RoleName = EntityManager.GetComponent<MetaDataComponent>(target).EntityName;
            comp.RoleDescription = Loc.GetString("station-event-random-sentience-role-description", ("name", comp.RoleName));
            RemComp<ReplacementAccentComponent>(target);
            RemComp<MonkeyAccentComponent>(target);
            EnsureComp<PotentialPsionicComponent>(target);
            EnsureComp<GhostTakeoverAvailableComponent>(target);
        }
    }
}
