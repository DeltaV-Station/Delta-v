using Content.Shared.Mind;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Tips.Conditions;

/// <summary>
/// Condition that checks if the player's mind has a specific role type.
/// Used for checking antag status (e.g., "SoloAntagonist", "TeamAntagonist", "FreeAgent").
/// </summary>
public sealed partial class HasRoleTypeCondition : TipCondition
{
    /// <summary>
    /// The role type prototype to check for.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<RoleTypePrototype> RoleType;

    protected override bool EvaluateImplementation(TipConditionContext ctx)
    {
        // This needs to be checked server-side
        // Return true here as a placeholder - actual check is in server TipSystem
        return true;
    }
}
