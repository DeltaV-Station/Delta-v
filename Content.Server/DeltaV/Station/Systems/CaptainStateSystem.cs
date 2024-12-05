using Content.Server.Chat.Systems;
using Content.Server.DeltaV.Cabinet;
using Content.Server.DeltaV.Station.Components;
using Content.Server.DeltaV.Station.Events;
using Content.Server.GameTicking;
using Content.Server.Station.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access;
using Content.Shared.Access.Systems;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server.DeltaV.Station.Systems;

public sealed class CaptainStateSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _reader = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StationJobsComponent, PlayerJobAddedEvent>(OnPlayerJobAdded);
        SubscribeLocalEvent<StationJobsComponent, PlayerJobsRemovedEvent>(OnPlayerJobsRemoved);
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _ticker.RoundDuration(); // Caching to reduce redundant calls
        var query = EntityQueryEnumerator<CaptainStateComponent>();
        while (query.MoveNext(out var station, out var captainState))
        {
            if (captainState.HasCaptain)
                HandleHasCaptain(station, captainState);
            else
                HandleNoCaptain(station, captainState, currentTime);
        }
    }

    private void OnPlayerJobAdded(Entity<StationJobsComponent> ent, ref PlayerJobAddedEvent args)
    {
        if (args.JobPrototypeId == "Captain")
        {
            if (TryComp<CaptainStateComponent>(ent, out var component))
                component.HasCaptain = true;
        }
    }

    private void OnPlayerJobsRemoved(Entity<StationJobsComponent> ent, ref PlayerJobsRemovedEvent args)
    {
        if (args.PlayerJobs == null || !args.PlayerJobs.Contains("Captain")) // If the player that left was a captain we need to check if there are any captains left
            return;
        if (ent.Comp.PlayerJobs.Count != 0 && ent.Comp.PlayerJobs.Any(playerJobs => playerJobs.Value.Contains("Captain"))) // We check the PlayerJobs if there are any cpatins left
            return;
        if (!TryComp<CaptainStateComponent>(ent, out var component)) // We update CaptainState if the station has one on the new captain status
            return;
        component.HasCaptain = false;
        component.TimeSinceCaptain = _ticker.RoundDuration();
        component.UnlockAA = false; // Captain has already brought AA in the round and should have resolved staffing issues already.
        component.ACORequestDelay = TimeSpan.Zero; // Expedite the voting process due to midround and captain equipment being in play.
    }

    /// <summary>
    /// Handles cases for when there is a captain
    /// </summary>
    /// <param name="station"></param>
    /// <param name="captainState"></param>
    private void HandleHasCaptain(Entity<CaptainStateComponent?> station, CaptainStateComponent captainState)
    {
        // If ACO vote has been called we need to cancel and alert to return to normal chain of command
        if (!captainState.IsACORequestActive)
            return;
        _chat.DispatchStationAnnouncement(station, Loc.GetString(captainState.RevokeACOMessage), colorOverride: Color.Gold);
        captainState.IsACORequestActive = false;
    }

    /// <summary>
    /// Handles cases for when there is no captain
    /// </summary>
    /// <param name="station"></param>
    /// <param name="captainState"></param>
    private void HandleNoCaptain(Entity<CaptainStateComponent?> station, CaptainStateComponent captainState, TimeSpan currentTime)
    {
        if (CheckACORequest(captainState, currentTime))
        {
            var message = captainState.UnlockAA ? captainState.ACORequestWithAAMessage : captainState.ACORequestNoAAMessage;
            _chat.DispatchStationAnnouncement(station, Loc.GetString(message, ("minutes", captainState.UnlockAADelay.TotalMinutes)), colorOverride: Color.Gold);
            captainState.IsACORequestActive = true;
        }
        if (CheckUnlockAA(captainState, currentTime))
        {
            captainState.UnlockAA = false; // Once unlocked don't unlock again
            _chat.DispatchStationAnnouncement(station, Loc.GetString(captainState.AAUnlockedMessage), colorOverride: Color.Red);

            // Extend access of spare id lockers to command so they can access emergency AA
            var query = EntityQueryEnumerator<SpareIDSafeComponent>();
            while (query.MoveNext(out var spareIDSafe, out var _))
            {
                if (!TryComp<AccessReaderComponent>(spareIDSafe, out var accessReader))
                    continue;
                var acceses = accessReader.AccessLists;
                if (acceses.Count <= 0) // Avoid restricting access for readers with no accesses
                    continue;
                // Awful and disgusting but the accessReader has no proper api for adding acceses to readers without awful type casting. See AccessOverriderSystem
                acceses.Add(new HashSet<ProtoId<AccessLevelPrototype>> { captainState.ACOAccess });
                Dirty(spareIDSafe, accessReader);
                RaiseLocalEvent(spareIDSafe, new AccessReaderConfigurationChangedEvent());
            }
        }
    }

    /// <summary>
    /// Checks the conditions for if an ACO should be requested
    /// </summary>
    /// <param name="captainState"></param>
    /// <returns>True if conditions are met for an ACO to be requested, False otherwise</returns>
    private bool CheckACORequest(CaptainStateComponent captainState, TimeSpan currentTime)
    {
        return !captainState.IsACORequestActive && currentTime > captainState.TimeSinceCaptain + captainState.ACORequestDelay;
    }

    /// <summary>
    /// Checks the conditions for if AA should be unlocked
    /// </summary>
    /// <param name="captainState"></param>
    /// <returns>True if conditions are met for AA to be unlocked, False otherwise</returns>
    private bool CheckUnlockAA(CaptainStateComponent captainState, TimeSpan currentTime)
    {
        return captainState.UnlockAA && currentTime > captainState.TimeSinceCaptain + captainState.ACORequestDelay + captainState.UnlockAADelay;
    }
}
