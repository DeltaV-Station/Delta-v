using Content.Shared.Players.PlayTimeTracking;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Tips.Conditions;

/// <summary>
/// Condition that checks if the player has MORE than a specified amount of playtime
/// on a specific playtime tracker.
/// </summary>
public sealed partial class MinPlaytimeCondition : TipCondition
{
    /// <summary>
    /// The playtime tracker prototype ID to check.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<PlayTimeTrackerPrototype> Tracker;

    /// <summary>
    /// Minimum playtime required on this tracker.
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
