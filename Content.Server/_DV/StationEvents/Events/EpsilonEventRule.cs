using System.Threading;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using JetBrains.Annotations;
using Robust.Shared.Player;
using Content.Server.AlertLevel;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.StationEvents.Events
{
    [UsedImplicitly]
    public sealed class EpsilonEventRule : StationEventSystem<EpsilonEventRuleComponent>
    {
        [Dependency] private readonly ApcSystem _apcSystem = default!;
        [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;

        protected override void Started(EntityUid uid, EpsilonEventRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
        {
            base.Started(uid, component, gameRule, args);

            component.AnnounceCancelToken?.Cancel();
            component.AnnounceCancelToken = new CancellationTokenSource();
            Timer.Spawn(10, () =>
            {
                Audio.PlayGlobal(component.PowerOffSound, Filter.Broadcast(), true);
            }, component.AnnounceCancelToken.Token);

            if (!TryGetRandomStation(out var chosenStation))
                return;

            component.AffectedStation = chosenStation.Value;

            var query = AllEntityQuery<ApcComponent, TransformComponent>();
            while (query.MoveNext(out var apcUid, out var apc, out var transform))
            {
                // Toggle all APCs on the station off, store for later
                if (apc.MainBreakerEnabled && CompOrNull<StationMemberComponent>(transform.GridUid)?.Station == chosenStation)
                {
                    _apcSystem.ApcToggleBreaker(apcUid, apc);
                    component.ToggledAPCs.Add((apcUid, apc));
                }
            }
        }

        protected override void Ended(EntityUid uid, EpsilonEventRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
        {
            if (TryComp(component.AffectedStation, out AlertLevelComponent? _))
            {
                _alertLevelSystem.SetLevel(component.AffectedStation, "epsilon", true, true, true);
            }

            // Toggle all disabled APCs back on
            foreach (var apc in component.ToggledAPCs)
            {
                if (!Deleted(apc) && !apc.Comp.MainBreakerEnabled)
                    _apcSystem.ApcToggleBreaker(apc, apc);
            }
            component.ToggledAPCs.Clear();
        }
    }
}
