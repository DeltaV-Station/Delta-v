using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Traits.Conditions;

/// <summary>
/// Condition that checks trait conflicts and requirements.
/// - Conflicts: if any listed trait is selected, this condition fails.
/// - Requires: all listed traits must be selected for this condition to pass.
/// Tooltips are generated per-entry by the UI rather than via GetTooltip,
/// so GetTooltip returns empty to avoid duplication.
/// </summary>
public sealed partial class TraitDependencyCondition : BaseTraitCondition
{
    /// <summary>
    /// Traits that are mutually exclusive with this one.
    /// If any of these are selected, this condition fails.
    /// </summary>
    [DataField]
    public List<ProtoId<TraitPrototype>> Conflicts = new();

    /// <summary>
    /// Traits that must already be selected for this condition to pass.
    /// All listed traits must be present.
    /// </summary>
    [DataField]
    public List<ProtoId<TraitPrototype>> Requires = new();

    protected override bool EvaluateImplementation(TraitConditionContext ctx)
    {
        if (ctx.SelectedTraits is { } selectedTraits)
        {
            foreach (var conflict in Conflicts)
            {
                if (selectedTraits.Contains(conflict))
                    return false;
            }
        }

        if (Requires.Count > 0)
        {
            if (ctx.SelectedTraits is not { } selected)
                return false;

            foreach (var required in Requires)
            {
                if (!selected.Contains(required))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns empty. the UI generates conflict/require tooltips directly via
    /// <see cref="GetTooltips"/> to avoid duplicating lines across the two tooltip paths.
    /// </summary>
    public override string GetTooltip(IPrototypeManager proto, ILocalizationManager loc) => string.Empty;

    /// <summary>
    /// Returns individual tooltip lines for each conflict and requirement.
    /// Used by the UI to build both the requirements preview and the failed-conditions tooltip.
    /// </summary>
    public IEnumerable<string> GetTooltips(IPrototypeManager proto, ILocalizationManager loc)
    {
        foreach (var conflict in Conflicts)
        {
            if (!proto.TryIndex(conflict, out var traitProto))
                continue;
            yield return loc.GetString("trait-condition-trait-conflict", ("trait", loc.GetString(traitProto.Name)));
        }

        foreach (var required in Requires)
        {
            if (!proto.TryIndex(required, out var traitProto))
                continue;
            yield return loc.GetString("trait-condition-trait-required", ("trait", loc.GetString(traitProto.Name)));
        }
    }
}
