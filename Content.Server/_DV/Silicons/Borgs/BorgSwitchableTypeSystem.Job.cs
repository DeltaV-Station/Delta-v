using Content.Server.Administration.Managers;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.Silicons.Borgs;

/// <summary>
/// Looks up role bans for players to prevent malf clients bypassing checks.
/// </summary>
public sealed partial class BorgSwitchableTypeSystem
{
    [Dependency] private readonly IBanManager _banMan = default!;
    [Dependency] private readonly PlayTimeTrackingSystem _playTime = default!;

    protected override FormattedMessage? IsJobAllowed(ICommonSession session, JobPrototype job)
    {
        if (_banMan.GetJobBans(session.UserId) is {} bans && bans.Contains(job.ID))
            return new FormattedMessage(); // server doesn't use it

        return _playTime.IsAllowed(session, job)
            ? null
            : new FormattedMessage(); // server doesn't use it
    }
}
