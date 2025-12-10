using Content.Server._DV.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._DV.StationEvents.GameRules;

/// <summary>
/// Mutes everyone for a random amount of time.
/// </summary>
internal sealed class NoosphericSilenceRule : StationEventSystem<NoosphericSilenceRuleComponent>
{
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;

    protected override void Started(EntityUid uid, NoosphericSilenceRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<PotentialPsionicComponent, MobStateComponent>();
        while (query.MoveNext(out var potPsion, out _, out _))
        {
            if (!_mobStateSystem.IsAlive(potPsion))
                continue;

            var ev = new TargetedByPsionicPowerEvent();
            RaiseLocalEvent(potPsion, ref ev);

            if (!ev.IsShielded)
                Silence(potPsion, component);
        }
    }

    private void Silence(EntityUid potPsion, NoosphericSilenceRuleComponent ruleComp)
    {
        var duration = _robustRandom.Next(ruleComp.MinDuration, ruleComp.MaxDuration);

        // TODO Replace with statusEffectSystemNew when Upstream makes a muted prototype.
        _statusEffectsSystem.TryAddStatusEffect(potPsion,
            "Muted",
            duration,
            false,
            "Muted");
    }
}
