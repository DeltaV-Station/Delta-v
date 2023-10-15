using System.Diagnostics.CodeAnalysis;
using Content.Shared.CCVar;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Robust.Shared.Utility;

namespace Content.Client.Players.PlayTimeTracking;

public sealed partial class JobRequirementsManager
{
    private bool _whitelisted = false;

    private bool CheckWhitelist(JobPrototype job, [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        if (!job.WhitelistRequired || !_cfg.GetCVar(CCVars.GameWhitelistJobs))
            return true;

        if (job.WhitelistRequired && _cfg.GetCVar(CCVars.GameWhitelistJobs) && !_whitelisted)
            reason = FormattedMessage.FromMarkup(Loc.GetString("playtime-deny-reason-not-whitelisted"));

        return reason == null;
    }

    private void RxWhitelist(MsgWhitelist message)
    {
        _whitelisted = message.Whitelisted;
    }
}
