using System.Text;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Traits.Conditions;

/// <summary>
/// Condition that passes if ANY of the child conditions pass.
/// Use this to create "must meet at least one of these requirements" checks.
/// </summary>
public sealed partial class AnyOfCondition : BaseTraitCondition
{
    /// <summary>
    /// List of conditions to check. Passes if any condition evaluates to true.
    /// </summary>
    [DataField(required: true)]
    public List<BaseTraitCondition> Conditions = new();

    protected override bool EvaluateImplementation(TraitConditionContext ctx)
    {
        // Inversion doesn't make sense for AnyOfCondition - use inverted child conditions instead
        if (Invert)
        {
            throw new InvalidOperationException(
                "AnyOfCondition does not support Invert. To require none of the conditions, " +
                "invert the individual child conditions instead.");
        }

        // Empty list should fail
        if (Conditions.Count == 0)
            return false;

        // Return true if ANY condition passes
        foreach (var condition in Conditions)
        {
            if (condition.Evaluate(ctx))
                return true;
        }

        return false;
    }

    public override string GetTooltip(IPrototypeManager proto, ILocalizationManager loc, int depth)
    {
        if (Conditions.Count == 0)
            return string.Empty;

        var requirementsTooltip = new StringBuilder();

        foreach (var condition in Conditions)
        {
            var conditionTooltip = condition.GetTooltip(proto, loc, depth + 1);
            if (conditionTooltip.Length > 0)
                requirementsTooltip.Append(conditionTooltip);
        }

        if (requirementsTooltip.Length == 0)
            return string.Empty;

        var tooltip = loc.GetString("trait-condition-any-of", ("requirements", requirementsTooltip));

        return new string(' ', depth * 2) + "- " + tooltip;
    }
}
