using Content.Server.Chat.Systems;
using Content.Server._DV.Cabinet;
using Content.Server._DV.Station.Components;
using Content.Server._DV.Station.Events;
using Content.Server.GameTicking;
using Content.Server.Station.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access;
using Content.Shared._DV.CCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Content.Server._DV.Station.Systems;

public sealed class CaptainStateSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private bool _aaEnabled;
    private bool _acoOnDeparture;
    private TimeSpan _aaDelay;
    private TimeSpan _acoDelay;

    public override void Initialize()
    {
        SubscribeLocalEvent<CaptainStateComponent, PlayerJobAddedEvent>(OnPlayerJobAdded);
        SubscribeLocalEvent<CaptainStateComponent, PlayerJobsRemovedEvent>(OnPlayerJobsRemoved);
        Subs.CVar(_cfg, DCCVars.AutoUnlockAllAccessEnabled, a => _aaEnabled = a, true);
        Subs.CVar(_cfg, DCCVars.RequestAcoOnCaptainDeparture, a => _acoOnDeparture = a, true);
        Subs.CVar(_cfg, DCCVars.AutoUnlockAllAccessDelay, a => _aaDelay = TimeSpan.FromMinutes(a), true);
        Subs.CVar(_cfg, DCCVars.RequestAcoDelay, a => _acoDelay = TimeSpan.FromMinutes(a), true);
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _ticker.RoundDuration(); // Caching to reduce redundant calls
        if (currentTime < _acoDelay) // Avoid timing issues. No need to run before _acoDelay is reached anyways.
            return;
        var query = EntityQueryEnumerator<CaptainStateComponent>();
        while (query.MoveNext(out var station, out var captainState))
        {
            if (currentTime < _acoDelay && captainState.IsACORequestActive == true) // Avoid timing issues. No need to run before _acoDelay is reached anyways.
            {
                Log.Error($"{captainState} IsACORequestActive true before ACO request time.");
                captainState.IsACORequestActive = false;
            }

            if (captainState.HasCaptain)
                HandleHasCaptain(station, captainState);
            else
                HandleNoCaptain(station, captainState, currentTime);
        }
    }

    private void OnPlayerJobAdded(Entity<CaptainStateComponent> ent, ref PlayerJobAddedEvent args)
    {
        if (args.JobPrototypeId == "Captain")
        {
            ent.Comp.IsAAInPlay = true;
            ent.Comp.HasCaptain = true;
        }
    }

    private void OnPlayerJobsRemoved(Entity<CaptainStateComponent> ent, ref PlayerJobsRemovedEvent args)
    {
        if (!TryComp<StationJobsComponent>(ent, out var stationJobs))
            return;
        if (!args.PlayerJobs.Contains("Captain")) // If the player that left was a captain we need to check if there are any captains left
            return;
        if (stationJobs.PlayerJobs.Any(playerJobs => playerJobs.Value.Contains("Captain"))) // We check the PlayerJobs if there are any cpatins left
            return;
        ent.Comp.HasCaptain = false;
        if (_acoOnDeparture)
        {
            _chat.DispatchStationAnnouncement(
                ent,
                Loc.GetString(ent.Comp.ACORequestNoAAMessage),
                colorOverride: Color.Gold);

            ent.Comp.IsACORequestActive = true;
        }
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

        _chat.DispatchStationAnnouncement(
            station,
            Loc.GetString(captainState.RevokeACOMessage),
            colorOverride: Color.Gold);

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
            var message =
                CheckUnlockAA(captainState, null)
                ? captainState.ACORequestWithAAMessage
                : captainState.ACORequestNoAAMessage;

            _chat.DispatchStationAnnouncement(
                station,
                Loc.GetString(message, ("minutes", _aaDelay.TotalMinutes)),
                colorOverride: Color.Gold);

            captainState.IsACORequestActive = true;
        }
        if (CheckUnlockAA(captainState, currentTime))
        {
            captainState.IsAAInPlay = true;
            _chat.DispatchStationAnnouncement(station, Loc.GetString(captainState.AAUnlockedMessage), colorOverride: Color.Red);

            // Extend access of spare id lockers to command so they can access emergency AA
            var query = EntityQueryEnumerator<SpareIDSafeComponent>();
            while (query.MoveNext(out var spareIDSafe, out _))
            {
                if (!TryComp<AccessReaderComponent>(spareIDSafe, out var accessReader))
                    continue;
                var accesses = accessReader.AccessLists;
                if (accesses.Count <= 0) // Avoid restricting access for readers with no accesses
                    continue;
                // Awful and disgusting but the accessReader has no proper api for adding acceses to readers without awful type casting. See AccessOverriderSystem
                accesses.Add(new HashSet<ProtoId<AccessLevelPrototype>> { captainState.ACOAccess });
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
        return !captainState.IsACORequestActive && currentTime > _acoDelay;
    }

    /// <summary>
    /// Checks the conditions for if AA should be unlocked
    /// If time is null its condition is ignored
    /// </summary>
    /// <param name="captainState"></param>
    /// <returns>True if conditions are met for AA to be unlocked, False otherwise</returns>
    private bool CheckUnlockAA(CaptainStateComponent captainState, TimeSpan? currentTime)
    {
        if (captainState.IsAAInPlay || !_aaEnabled)
            return false;
        return currentTime == null || currentTime > _acoDelay + _aaDelay;
    }
}
