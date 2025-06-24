using Content.Shared.Roles;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Silicons.Borgs;

/// <summary>
/// Handles checking job requirements and role bans for borg types.
/// </summary>
public abstract partial class SharedBorgSwitchableTypeSystem
{
    /// <summary>
    /// Checks requirements for selecting a borg type.
    /// </summary>
    /// <returns>Null if successful, an error message if not.</returns>
    public FormattedMessage? TrySelect(EntityUid uid, ProtoId<BorgTypePrototype> id)
    {
        var proto = Prototypes.Index(id);
        if (proto.Job is not {} jobId)
            return null; // nothing to check

        // using an action requires a session so this should never fail
        var session = Comp<ActorComponent>(uid).PlayerSession;
        return IsJobAllowed(session, Prototypes.Index(jobId));
    }

    /// <summary>
    /// Return null for success, non-null for failure.
    /// Server does not return an actual message, client does.
    /// Client and server have different implementations because it's not just in shared.
    /// </summary>
    protected abstract FormattedMessage? IsJobAllowed(ICommonSession session, JobPrototype job);
}
