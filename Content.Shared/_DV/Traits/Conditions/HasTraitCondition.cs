using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Traits.Conditions;

/// <summary>
/// Condition that checks if the player has activated a specific trait.
/// </summary>
public sealed partial class HasTraitCondition : BaseTraitCondition
{
    /// <summary>
    /// List of conditions to check. Passes if any condition evaluates to true.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<TraitPrototype> Trait = default!;

    protected override bool EvaluateImplementation(TraitConditionContext ctx)
    {
        if (ctx.Profile is not { } profile)
            return false;
        return profile
            .GetValidTraits(ctx.Profile.TraitPreferences, ctx.Proto)
            .Contains(Trait);
    }

    public override string GetTooltip(IPrototypeManager proto, ILocalizationManager loc, int depth)
    {
        if (!proto.TryIndex(Trait, out var trait))
            return string.Empty;

        var tooltip = Invert
            ? loc.GetString("trait-condition-trait-has-not", ("trait", loc.GetString(trait.Name)))
            : loc.GetString("trait-condition-trait-has", ("trait", loc.GetString(trait.Name)));

        return new string(' ', depth * 2) + "- " + tooltip + Environment.NewLine;
    }
}
