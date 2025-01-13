using Content.Server._DV.Chemistry.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Nutrition.EntitySystems;

namespace Content.Server._DV.Nutrition.EntitySystems;

public sealed class BlockDrinkingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlockDrinkingComponent, UseInHandEvent>(OnUse, before: [typeof(SharedDrinkSystem)]);
        SubscribeLocalEvent<BlockDrinkingComponent, AfterInteractEvent>(AfterInteract,
            before: [typeof(SharedDrinkSystem)]);
    }

    private void OnUse(Entity<BlockDrinkingComponent> entity, ref UseInHandEvent args)
    {
        args.Handled = true;
    }

    private void AfterInteract(Entity<BlockDrinkingComponent> entity, ref AfterInteractEvent args)
    {
        args.Handled = true;
    }
}
