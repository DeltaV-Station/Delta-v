using Content.Server._DV.Psionics.Systems;
using Content.Server._DV.StationEvents.Components;
using Content.Server.Popups;
using Content.Server.StationEvents.Events;
using Content.Shared._DV.Psionics.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Stunnable;

namespace Content.Server._DV.StationEvents.GameRules;

/// <summary>
/// Zaps everyone, rolling psionics and disorienting them
/// </summary>
internal sealed class NoosphericZapRule : StationEventSystem<NoosphericZapRuleComponent>
{
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly PsionicSystem _psionic = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedStutteringSystem _stuttering = default!;

    protected override void Started(EntityUid uid, NoosphericZapRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<PotentialPsionicComponent, MobStateComponent>();

        while (query.MoveNext(out var potPsion, out var potPsionComponent, out var mobStateComponent))
        {
            if (!_mobStateSystem.IsAlive(potPsion, mobStateComponent))
                continue;

            if (!_psionic.CanBeTargeted(potPsion))
                continue;

            // Zap non-psionics only if they spent their roll already.
            if (potPsionComponent.Rolled)
                Zap(potPsion, potPsionComponent);
            // Then zap all other psionics regardless.
            else if (HasComp<PsionicComponent>(potPsion))
                Zap(potPsion, potPsionComponent);
        }
    }

    private void Zap(EntityUid psionic, PotentialPsionicComponent potPsionComp)
    {
        _stun.TryUpdateParalyzeDuration(psionic, TimeSpan.FromSeconds(5));
        _jittering.DoJitter(psionic, TimeSpan.FromSeconds(5), false);
        _stuttering.DoStutter(psionic, TimeSpan.FromSeconds(15), false);

        var grantedRoll = _psionic.GrantPsionicRoll((psionic, potPsionComp));
        var message = grantedRoll
            ? "gamerule-noospheric-zap-seize-potential-regained"
            : "gamerule-noospheric-zap-seize";

        _popupSystem.PopupEntity(Loc.GetString(message), psionic, psionic, PopupType.LargeCaution);
    }
}
