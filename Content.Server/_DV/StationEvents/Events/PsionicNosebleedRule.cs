using Content.Server._DV.StationEvents.Components;
using Content.Server.Body.Systems;
using Content.Server.StationEvents.Events;
using Content.Shared._DV.Psionics.Components;
using Content.Shared.Abilities.Psionics;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Server._DV.StationEvents.Events;

public sealed class PsionicNosebleedRule : StationEventSystem<PsionicNosebleedRuleComponent>
{
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    protected override void Started(EntityUid uid, PsionicNosebleedRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var query = EntityQueryEnumerator<PsionicComponent, MobStateComponent>();
        while (query.MoveNext(out var psion, out _, out var mobState))
        {
            if (_mob.IsAlive(psion, mobState) && !HasComp<PsionicInsulationComponent>(psion))
            {
                _popup.PopupEntity(Loc.GetString("psionic-nosebleed-message"), psion, psion, PopupType.MediumCaution);
                _bloodstream.TryModifyBleedAmount(psion, comp.BleedAmount);
            }
        }
    }
}
