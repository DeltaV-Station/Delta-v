using Content.Shared.Inventory.Events;

namespace Content.Server.SimpleStation14.Clothing;

public sealed class DeleteOnEquipSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeleteOnEquipComponent, GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<DeleteOnEquipComponent, GotUnequippedEvent>(OnUnequip);
    }

    private void OnEquip(EntityUid uid, DeleteOnEquipComponent component, GotEquippedEvent args)
    {
        if (component.OnEquip == true) QueueDel(args.Equipment);
    }

    private void OnUnequip(EntityUid uid, DeleteOnEquipComponent component, GotUnequippedEvent args)
    {
        if (component.OnEquip == false) QueueDel(args.Equipment);
    }
}