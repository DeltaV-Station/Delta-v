using Robust.Shared.Random;
using Content.Server.Abilities.Psionics;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Server.Psionics;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Mobs.Systems;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Zombies;

namespace Content.Server.StationEvents.Events;

internal sealed class NoosphericStormRule : StationEventSystem<NoosphericStormRuleComponent>
{
    [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilitiesSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    protected override void Started(EntityUid uid, NoosphericStormRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        List<EntityUid> validList = new();

        var query = EntityManager.EntityQueryEnumerator<PotentialPsionicComponent>();
        while (query.MoveNext(out var potentialPsionic, out var potentialPsionicComponent))
        {
            if (_mobStateSystem.IsDead(potentialPsionic))
                continue;

            // Skip over those who are already psionic or those who are insulated, or zombies.
            if (HasComp<PsionicComponent>(potentialPsionic) || HasComp<PsionicInsulationComponent>(potentialPsionic) || HasComp<ZombieComponent>(potentialPsionic))
                continue;

            validList.Add(potentialPsionic);
        }

        // Give some targets psionic abilities.
        RobustRandom.Shuffle(validList);

        var toAwaken = RobustRandom.Next(1, component.MaxAwaken);

        foreach (var target in validList)
        {
            if (toAwaken-- == 0)
                break;

            _psionicAbilitiesSystem.AddPsionics(target);
        }

        // Increase glimmer.
        var baseGlimmerAdd = _robustRandom.Next(component.BaseGlimmerAddMin, component.BaseGlimmerAddMax);
        //var glimmerSeverityMod = 1 + (component.GlimmerSeverityCoefficient * (GetSeverityModifier() - 1f));
        var glimmerAdded = (int) baseGlimmerAdd; // Math.Round(baseGlimmerAdd * glimmerSeverityMod);

        _glimmerSystem.Glimmer += glimmerAdded;
    }
}
