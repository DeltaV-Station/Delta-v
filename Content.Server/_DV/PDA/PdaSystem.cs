using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server._DV.Shuttles.Events;
using Content.Server.Shuttles.Events;
using Content.Shared.PDA;

namespace Content.Server.PDA
{
    public sealed partial class PdaSystem
    {
        [Dependency] private readonly RoundEndSystem _roundEnd = default!;
        [Dependency] private readonly EmergencyShuttleSystem _evacShuttle = default!;

        private void InitializeExtras()
        {
            SubscribeLocalEvent<RoundEndSystemChangedEvent>(OnRoundEndChanged);
            SubscribeLocalEvent<EvacShuttleDockedEvent>(OnShuttleDockedEvent);
            SubscribeLocalEvent<EmergencyShuttleAuthorizedEvent>(OnShuttleEarlyLaunch);
        }

        private void OnRoundEndChanged(RoundEndSystemChangedEvent ev)
        {
            // When we get a change to the round end state, update all PDAs with new shuttle evac time
            // ExpectedCountdownEnd can be null which means no shuttle is coming (it was recalled)
            var query = AllEntityQuery<PdaComponent>();
            while (query.MoveNext(out var ent, out var comp))
            {
                comp.EvacArrivalTime = _roundEnd.ExpectedCountdownEnd;
                UpdatePdaUi(ent, comp);
            }
        }

        private void OnShuttleDockedEvent(EvacShuttleDockedEvent ev)
        {
            // Whenever the evac shuttle docks with the station, update all PDAs with the departure time
            var query = AllEntityQuery<PdaComponent>();
            while (query.MoveNext(out var ent, out var comp))
            {
                comp.EvacDepartureTime = _evacShuttle.EvacShuttleDepartureTime;
                UpdatePdaUi(ent, comp);
            }
        }

        private void OnShuttleEarlyLaunch(EmergencyShuttleAuthorizedEvent ev)
        {
            // If evac early launch is activated, update the departure time
            var query = AllEntityQuery<PdaComponent>();
            while (query.MoveNext(out var ent, out var comp))
            {
                comp.EvacDepartureTime = _evacShuttle.EvacShuttleDepartureTime;
                UpdatePdaUi(ent, comp);
            }
        }
    }
}
