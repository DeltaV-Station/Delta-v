using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Tips.Conditions;

/// <summary>
/// Condition that checks if the player has a specific job.
/// </summary>
public sealed partial class HasJobCondition : TipCondition
{
    /// <summary>
    /// The job prototype ID to check for.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<JobPrototype> Job;

    protected override bool EvaluateImplementation(TipConditionContext ctx)
    {
        // This needs to be checked server-side with JobSystem
        // Return true here as a placeholder - actual check is in server TipSystem
        return true;
    }
}
