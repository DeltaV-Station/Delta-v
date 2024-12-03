using Content.Server.Chat.Systems;
using Content.Server.DeltaV.Station.Components;
using Content.Server.GameTicking;
using Content.Server.Station.Systems;
using Content.Shared.Access.Systems;

namespace Content.Server.DeltaV.Station.Systems;

public sealed class CaptainStateSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _ticker.RoundDuration(); // Caching to reduce redundant calls
        var query = EntityQueryEnumerator<CaptainStateComponent>();
        while (query.MoveNext(out var station, out var captainState))
        {
            if (captainState.HasCaptain == true)
                HandleHasCaptain(station, captainState);
            else
                HandleNoCaptain(station, captainState, currentTime);
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
        if (captainState.IsACORequestActive == false)
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
            _entity.System<AccessReaderSystem>().ExpandAccessForAllReaders(captainState.EmergencyAAAccess, captainState.ACOAccess);
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
