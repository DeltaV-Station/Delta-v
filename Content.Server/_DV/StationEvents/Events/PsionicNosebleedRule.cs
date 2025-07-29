using Content.Server._DV.StationEvents.Components;
using Content.Server.Body.Systems;
using Content.Server.Psionics.Glimmer;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Events;
using Content.Shared.Abilities.Psionics;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;

namespace Content.Server._DV.StationEvents.Events;

public sealed class PsionicNosebleedRule : StationEventSystem<PsionicNosebleedRuleComponent>
{
    [Dependency] private readonly GlimmerSystem _glimmer = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly GlimmerReactiveSystem _glimmerReactiveSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    protected override void Started(EntityUid uid, PsionicNosebleedRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var query = EntityQueryEnumerator<PsionicComponent, MobStateComponent>();
        while (query.MoveNext(out var psion, out _, out _))
        {
            if (_mobStateSystem.IsAlive(psion) && !HasComp<PsionicInsulationComponent>(psion))
            {
                _popup.PopupEntity(Loc.GetString("psionic-nosebleed-message"), psion, psion, PopupType.MediumCaution);
                _bloodstreamSystem.TryModifyBleedAmount(psion, comp.BleedAmount);
            }
        }
    }
}
