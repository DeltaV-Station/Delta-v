using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Traits.Conditions;

/// <summary>
/// Condition that checks if the player has a specific component.
/// Use Invert = true to check if the player does NOT have the component.
/// </summary>
public sealed partial class HasCompCondition : BaseTraitCondition
{
    /// <summary>
    /// The component name to check for (e.g., "Pacifism").
    /// </summary>
    [DataField(required: true, customTypeSerializer: typeof(ComponentNameSerializer))]
    public string Component = string.Empty;

    protected override bool EvaluateImplementation(TraitConditionContext ctx)
    {
        if (string.IsNullOrEmpty(Component))
            return false;

        try
        {
            var compType = ctx.CompFactory.GetRegistration(Component).Type;
            return ctx.EntMan.HasComponent(ctx.Player, compType);
        }
        catch (Exception)
        {
            // Log the actual error instead of silently catching
            ctx.LogMan.GetSawmill("traits").Error($"Failed to get component registration for '{Component}'");
            return false;
        }
    }

    public override string GetTooltip(IPrototypeManager proto, ILocalizationManager loc)
    {
        // No tooltip for this condition since we're dealing with comps
        return string.Empty;
    }
}
