using Robust.Shared.Random;
using Robust.Shared.Player;
using Content.Server.Psionics;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Abilities.Psionics;
using Content.Shared.StatusEffect;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Server.StationEvents.Events;

/// <summary>
/// Mutes everyone for a random amount of time.
/// </summary>
internal sealed class PsionicCatGotYourTongueRule : StationEventSystem<PsionicCatGotYourTongueRuleComponent>
{
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly SharedAudioSystem _sharedAudioSystem = default!;


    protected override void Started(EntityUid uid, PsionicCatGotYourTongueRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        List<EntityUid> psionicList = new();

        var query = EntityQueryEnumerator<PotentialPsionicComponent, MobStateComponent>();
        while (query.MoveNext(out var psion, out _, out _))
        {
            if (_mobStateSystem.IsAlive(psion) && !HasComp<PsionicInsulationComponent>(psion))
                psionicList.Add(psion);
        }

        foreach (var psion in psionicList)
        {
            var duration = _robustRandom.Next(component.MinDuration, component.MaxDuration);

            _statusEffectsSystem.TryAddStatusEffect(psion,
                "Muted",
                duration,
                false,
                "Muted");

            _sharedAudioSystem.PlayGlobal(component.Sound, Filter.Entities(psion), false);
        }
    }
}
