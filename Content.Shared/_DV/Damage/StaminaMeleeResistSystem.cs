using Content.Shared.Armor;
using Content.Shared.Damage.Events;
using Content.Shared.Inventory;

namespace Content.Shared._DV.Damage;

public sealed class StaminaMeleeResistanceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArmorComponent, InventoryRelayedEvent<BeforeStaminaDamageEvent>>(OnGetMeleeResistance);
    }

    private void OnGetMeleeResistance(Entity<ArmorComponent> ent, ref InventoryRelayedEvent<BeforeStaminaDamageEvent> args)
    {
        if (args.Args.FromMelee)
            args.Args.Value *= ent.Comp.StaminaMeleeDamageCoefficient;
    }
}
