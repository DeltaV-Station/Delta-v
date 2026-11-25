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

    public override string GetTooltip(IPrototypeManager proto, ILocalizationManager loc)
    {
        if (Conditions.Count == 0)
            return string.Empty;

        var requirements = new List<string>();

        foreach (var condition in Conditions)
        {
            var tooltip = condition.GetTooltip(proto, loc);
            if (!string.IsNullOrEmpty(tooltip))
                requirements.Add(tooltip);
        }

        if (requirements.Count == 0)
            return string.Empty;

        // AAAAAAAAAAA
        var joinedRequirements = string.Join("\n• ", requirements);

        // Handle inversion in tooltip (ANY becomes NONE when inverted)
        return Loc.GetString("trait-condition-any-of", ("requirements", joinedRequirements));
    }
}
