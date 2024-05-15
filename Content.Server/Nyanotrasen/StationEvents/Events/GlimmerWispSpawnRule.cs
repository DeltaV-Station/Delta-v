using System.Linq;
using Content.Server.GameTicking.Components;
using Content.Server.Psionics.Glimmer;
using Content.Server.StationEvents;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.Abilities.Psionics;
using Content.Shared.NPC.Components;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Random;

namespace Content.Server.Nyanotrasen.StationEvents.Events;

internal sealed class GlimmerWispRule : StationEventSystem<GlimmerWispRuleComponent>
{
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;

    private static readonly string WispPrototype = "MobGlimmerWisp";

    protected override void Started(EntityUid uid, GlimmerWispRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var glimmerSources = EntityManager.EntityQuery<GlimmerSourceComponent, TransformComponent>().ToList();
        var normalSpawnLocations = EntityManager.EntityQuery<VentCritterSpawnLocationComponent, TransformComponent>().ToList();
        var hiddenSpawnLocations = EntityManager.EntityQuery<MidRoundAntagSpawnLocationComponent, TransformComponent>().ToList();

        var baseCount = Math.Max(1, EntityManager.EntityQuery<PsionicComponent, NpcFactionMemberComponent>().Count() / 10);
        int multiplier = Math.Max(1, (int) _glimmerSystem.GetGlimmerTier() - 2);

        var total = baseCount * multiplier;

        int i = 0;
        while (i < total)
        {
            if (glimmerSources.Count != 0 && _robustRandom.Prob(0.4f))
            {
                EntityManager.SpawnEntity(WispPrototype, _robustRandom.Pick(glimmerSources).Item2.Coordinates);
                i++;
                continue;
            }

            if (normalSpawnLocations.Count != 0)
            {
                EntityManager.SpawnEntity(WispPrototype, _robustRandom.Pick(normalSpawnLocations).Item2.Coordinates);
                i++;
                continue;
            }

            if (hiddenSpawnLocations.Count != 0)
            {
                EntityManager.SpawnEntity(WispPrototype, _robustRandom.Pick(hiddenSpawnLocations).Item2.Coordinates);
                i++;
                continue;
            }
            return;
        }
    }
}
