using Content.Server.DeltaV.Cargo.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Cargo.Systems;

public sealed partial class PricingSystem
{
    /// <summary>
    /// Applies any price modifiers defined in the entity prototype.
    /// </summary>
    /// <param name="prototype">The entity prototype.</param>
    /// <param name="basePrice">The base price before modification.</param>
    /// <returns>The modified price.</returns>
    private double ApplyPrototypePriceModifier(EntityPrototype prototype, double basePrice)
    {
        if (prototype.Components.TryGetValue(_factory.GetComponentName(typeof(PriceModifierComponent)),
                out var modProto))
        {
            var priceModifier = (PriceModifierComponent)modProto.Component;
            return basePrice * priceModifier.Modifier;
        }

        return basePrice;
    }

    /// <summary>
    /// Applies any price modifiers to the calculated price.
    /// </summary>
    /// <param name="uid">The entity whose price is being modified.</param>
    /// <param name="basePrice">The base price before modification.</param>
    /// <returns>The modified price.</returns>
    private double ApplyPriceModifier(EntityUid uid, double basePrice)
    {
        if (TryComp<PriceModifierComponent>(uid, out var modifier))
        {
            return basePrice * modifier.Modifier;
        }

        return basePrice;
    }
}
