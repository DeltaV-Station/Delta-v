using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Traits.Conditions;

/// <summary>
/// Condition that checks if the player is a specific species.
/// Use Invert = true to check if the player is NOT the species.
/// </summary>
public sealed partial class IsSpeciesCondition : BaseTraitCondition
{
    /// <summary>
    /// The species ID to check for.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<SpeciesPrototype> Species = string.Empty;

    protected override bool EvaluateImplementation(TraitConditionContext ctx)
    {
        if (string.IsNullOrEmpty(ctx.SpeciesId))
            return false;

        return ctx.SpeciesId == Species;
    }

    public override string GetTooltip(IPrototypeManager proto, ILocalizationManager loc)
    {
        var speciesName = Species.Id;
        if (proto.TryIndex(Species, out var speciesProto))
        {
            speciesName = loc.GetString(speciesProto.Name);
        }

        return Invert
            ? loc.GetString("trait-condition-species-not", ("species", speciesName))
            : loc.GetString("trait-condition-species-is", ("species", speciesName));
    }
}
