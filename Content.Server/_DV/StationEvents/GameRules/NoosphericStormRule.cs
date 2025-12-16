using System.Linq;
using Content.Server._DV.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Systems;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Random;

namespace Content.Server._DV.StationEvents.GameRules;

internal sealed class NoosphericStormRule : StationEventSystem<NoosphericStormRuleComponent>
{
    [Dependency] private readonly SharedPsionicSystem _psionic = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    protected override void Started(EntityUid uid, NoosphericStormRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        Dictionary<EntityUid, PotentialPsionicComponent> validList = [];

        var query = EntityManager.EntityQueryEnumerator<PotentialPsionicComponent>();
        while (query.MoveNext(out var potPsionic, out var potPsionicComp))
        {
            if (!_mobStateSystem.IsAlive(potPsionic)
                || HasComp<PsionicComponent>(potPsionic)) // Skip over already psionic entities.
                continue;

            var ev = new TargetedByPsionicPowerEvent();
            RaiseLocalEvent(potPsionic, ref ev);

            if (ev.IsShielded) // Skip over shielded entities.
                continue;

            validList.Add(potPsionic, potPsionicComp);
        }

        // Give some targets psionic abilities.
        var keyList = validList.Keys.ToList();
        _robustRandom.Shuffle(keyList);

        var toAwaken = RobustRandom.Next(1, component.MaxAwaken);
        var additional = _glimmerSystem.Glimmer / component.AdditionalAwokenPerGlimmer;
        toAwaken = (int) MathF.Round(toAwaken, 0, MidpointRounding.ToZero);

        foreach (var target in keyList.TakeWhile(_ => toAwaken-- != 0))
        {
            _psionic.AddRandomPsionicPower((target, validList[target]), midRound: true);
        }

        // Increase glimmer.
        var baseGlimmerAdd = _robustRandom.Next(component.BaseGlimmerAddMin, component.BaseGlimmerAddMax);
        //var glimmerSeverityMod = 1 + (component.GlimmerSeverityCoefficient * (GetSeverityModifier() - 1f));
        var glimmerAdded = baseGlimmerAdd; // Math.Round(baseGlimmerAdd * glimmerSeverityMod);

        _glimmerSystem.Glimmer += glimmerAdded;
    }
}
