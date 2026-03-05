using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Traits.Conditions;

/// <summary>
/// Condition that checks if the player has enabled a specific antag preference.
/// </summary>
public sealed partial class IsAntagEligibleCondition : BaseTraitCondition
{
    /// <summary>
    /// The antag prototype ID to check for eligibility.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<AntagPrototype> Antag;

    protected override bool EvaluateImplementation(TraitConditionContext ctx)
    {
        if (ctx.Profile == null)
            return false;

        // Check if the player has this antag preference enabled
        return ctx.Profile.AntagPreferences.Contains(Antag);
    }

    public override string GetTooltip(IPrototypeManager proto, ILocalizationManager loc)
    {
        if (!proto.TryIndex(Antag, out var antagProto))
            return string.Empty;

        var antagName = loc.GetString(antagProto.Name);

        return Invert
            ? loc.GetString("trait-condition-antag-not", ("antag", antagName))
            : loc.GetString("trait-condition-antag-is", ("antag",  antagName));
    }
}
