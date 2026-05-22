using Content.Shared.Players.PlayTimeTracking;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Tips.Conditions;

/// <summary>
/// Condition that checks if the player has LESS than a specified amount of playtime
/// on a specific playtime tracker.
/// Useful for showing tips to players new to a specific role.
/// </summary>
public sealed partial class MaxPlaytimeCondition : TipCondition
{
    /// <summary>
    /// The playtime tracker prototype ID to check.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<PlayTimeTrackerPrototype> Tracker;

    /// <summary>
    /// Maximum playtime allowed on this tracker.
    /// Player must have less than this to pass.
    /// </summary>
    [DataField(required: true)]
    public TimeSpan Time;

    protected override bool EvaluateImplementation(TipConditionContext ctx)
    {
        // This needs to be checked server-side with PlayTimeTrackingManager
        // Return true here as a placeholder - actual check is in server TipSystem
        return true;
    }
}
