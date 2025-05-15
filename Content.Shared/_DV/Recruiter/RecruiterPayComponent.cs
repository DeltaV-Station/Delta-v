using Content.Shared.NPC.Prototypes;
using Content.Shared.Storage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Recruiter;

/// <summary>
/// Component that handles payout for successful recruiting
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RecruiterPayComponent : Component
{

}
/// <summary>
/// Raised on an entity when it is successfully recruited by a recruiter pen
/// </summary>
[ByRefEvent]
public readonly record struct PayoutEvent(EntityUid? User)
{
    public readonly EntityUid? User = User;
}
