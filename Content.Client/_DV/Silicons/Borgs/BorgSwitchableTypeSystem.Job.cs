using Content.Client.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Client.Silicons.Borgs;

/// <summary>
/// Looks up local role bans.
/// </summary>
public sealed partial class BorgSwitchableTypeSystem
{
    [Dependency] private readonly JobRequirementsManager _jobRequirements = default!;

    protected override FormattedMessage? IsJobAllowed(ICommonSession session, JobPrototype job)
    {
        _jobRequirements.IsAllowed(job, profile: null, out var msg);
        return msg;
    }
}
