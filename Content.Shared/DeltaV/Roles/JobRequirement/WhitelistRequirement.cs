using System.Diagnostics.CodeAnalysis;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Roles;

/// <summary>
/// Requires the player be globally whitelisted to play a role.
/// </summary>
/// <remarks>
/// Don't use this for jobs, use <c>whitelisted: true</c> on the JobPrototype instead.
/// </remarks>
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
