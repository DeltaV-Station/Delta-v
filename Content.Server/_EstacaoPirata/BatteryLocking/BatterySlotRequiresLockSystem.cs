using Content.Shared.Containers.ItemSlots;
using Content.Shared.Lock;

namespace Content.Server.SimpleStation14.BatteryLocking;

public sealed class BatterySlotRequiresLockSystem : EntitySystem

{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BatterySlotRequiresLockComponent, LockToggledEvent>(LockToggled);

    }
    private void LockToggled(EntityUid uid, BatterySlotRequiresLockComponent component, LockToggledEvent args)
    {
        if (!TryComp<LockComponent>(uid, out var lockComp) || !TryComp<ItemSlotsComponent>(uid, out var itemslots))
            return;
        if (!_itemSlotsSystem.TryGetSlot(uid, component.ItemSlot, out var slot, itemslots))
            return;
        _itemSlotsSystem.SetLock(uid, slot, lockComp.Locked, itemslots);
    }
}
