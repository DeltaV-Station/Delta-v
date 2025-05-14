using Content.Shared.Armor;
using Content.Shared.Damage.Events;
using Content.Shared.Inventory;

namespace Content.Shared.Damage.Systems;

public sealed partial class StaminaSystem
{
    private void InitializeMeleeResistance()
    {
        SubscribeLocalEvent<ArmorComponent, InventoryRelayedEvent<BeforeStaminaDamageEvent>>(OnGetMeleeResistance);
    }

    private void OnGetMeleeResistance(Entity<ArmorComponent> ent, ref InventoryRelayedEvent<BeforeStaminaDamageEvent> args)
    {
        if (args.Args.FromMelee)
            args.Args.Value *= ent.Comp.StaminaMeleeDamageCoefficient;
    }
}
