using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Roles;

/// <summary>
/// Requires that the user is globally whitelisted to do this job.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class WhitelistRequirement : JobRequirement
{
    public override bool Check(IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason,
        bool isWhitelisted)
    {
        reason = null;
        if (isWhitelisted)
            return true;

        reason = FormattedMessage.FromMarkup(Loc.GetString("playtime-deny-reason-not-whitelisted"));
        return false;
    }
}
