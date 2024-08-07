using Content.Shared.Containers.ItemSlots;
using Content.Shared.Lock;
using Content.Shared.Popups;
using Content.Shared.SimpleStation14.Silicon.Components; // DeltaV
using Content.Shared.IdentityManagement; // DeltaV

namespace Content.Server.SimpleStation14.BatteryLocking;

public sealed class BatterySlotRequiresLockSystem : EntitySystem

{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!; // DeltaV

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BatterySlotRequiresLockComponent, LockToggledEvent>(LockToggled);
        SubscribeLocalEvent<BatterySlotRequiresLockComponent, LockToggleAttemptEvent>(LockToggleAttempted); // DeltaV

    }
    private void LockToggled(EntityUid uid, BatterySlotRequiresLockComponent component, LockToggledEvent args)
    {
        if (!TryComp<LockComponent>(uid, out var lockComp) || !TryComp<ItemSlotsComponent>(uid, out var itemslots))
            return;
        if (!_itemSlotsSystem.TryGetSlot(uid, component.ItemSlot, out var slot, itemslots))
            return;
        _itemSlotsSystem.SetLock(uid, slot, lockComp.Locked, itemslots);
    }

    // DeltaV - Alert IPCs when they are being unlocked
    private void LockToggleAttempted(EntityUid uid, BatterySlotRequiresLockComponent component, LockToggleAttemptEvent args)
    {
        if (args.User != uid && TryComp<SiliconComponent>(uid, out var siliconComp))
            _popupSystem.PopupEntity(Loc.GetString("batteryslotrequireslock-component-alert-owner", ("user", Identity.Entity(args.User, EntityManager))), uid, uid, PopupType.Large);
    }
    // End of DeltaV Code
}
