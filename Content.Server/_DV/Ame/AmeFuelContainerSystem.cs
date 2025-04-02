using Content.Shared.Ame.Components;
using Content.Server.Cargo.Systems;

namespace Content.Server._DV.Ame;

public sealed class AmeFuelContainerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AmeFuelContainerComponent, PriceCalculationEvent>(CalculateFuelPrice);
    }

    /// <summary>
    /// Gets the price for the fuel in container.
    /// </summary>
    private void CalculateFuelPrice(Entity<AmeFuelContainerComponent> ent, ref PriceCalculationEvent args)
    {
        args.Price += ent.Comp.FuelAmount * ent.Comp.PricePerFuel;
    }
}
