using System.Linq;
using Content.Server.Psionics.Glimmer;
using Content.Server.StationEvents;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.Abilities.Psionics;
using Content.Shared.GameTicking.Components;
using Content.Shared.NPC.Components;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Random;
using Robust.Shared.Map;

namespace Content.Server.DeltaV.StationEvents.Events;

public sealed class GlimmerMobRule : StationEventSystem<GlimmerMobRuleComponent>
{
    [Dependency] private readonly GlimmerSystem _glimmer = default!;

    protected override void Started(EntityUid uid, GlimmerMobRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var glimmerSources = GetCoords<GlimmerSourceComponent>();
        var normalSpawns = GetCoords<VentCritterSpawnLocationComponent>();
        var hiddenSpawns = GetCoords<MidRoundAntagSpawnLocationComponent>();

        var psionics = EntityQuery<PsionicComponent, NpcFactionMemberComponent>().Count();
        var baseCount = Math.Max(1, psionics / comp.MobsPerPsionic);
        int multiplier = Math.Max(1, (int) _glimmer.GetGlimmerTier() - (int) comp.GlimmerTier);

        var total = baseCount * multiplier;
        if (comp.MaxSpawns is {} maxSpawns)
            total = Math.Min(total, maxSpawns);

        Log.Info($"Spawning {total} of {comp.MobPrototype} from {ToPrettyString(uid):rule}");
        for (var i = 0; i < total; i++)
        {
            // if we cant get a spawn just give up
            if (!TrySpawn(comp, glimmerSources, comp.GlimmerProb) &&
                !TrySpawn(comp, normalSpawns, comp.NormalProb) &&
                !TrySpawn(comp, hiddenSpawns, comp.HiddenProb))
                return;
        }
    }

    private List<EntityCoordinates> GetCoords<T>() where T : IComponent
    {
        var coords = new List<EntityCoordinates>();
        var query = EntityQueryEnumerator<TransformComponent, T>();
        while (query.MoveNext(out var xform, out _))
        {
            coords.Add(xform.Coordinates);
        }

        return coords;
    }

    private bool TrySpawn(GlimmerMobRuleComponent comp, List<EntityCoordinates> spawns, float prob)
    {
        if (spawns.Count == 0 || !RobustRandom.Prob(prob))
            return false;

        var coords = RobustRandom.Pick(spawns);
        Spawn(comp.MobPrototype, coords);
        return true;
    }
}
