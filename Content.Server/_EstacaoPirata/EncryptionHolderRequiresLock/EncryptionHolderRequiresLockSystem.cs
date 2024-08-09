using Content.Shared.Containers.ItemSlots;
using Content.Shared.Lock;
using Content.Shared.Radio.Components;
using Content.Shared.Radio.EntitySystems;

namespace Content.Server.SimpleStation14.EncryptionHolderRequiresLock;

public sealed class EncryptionHolderRequiresLockSystem : EntitySystem

{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly EncryptionKeySystem _encryptionKeySystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EncryptionHolderRequiresLockComponent, LockToggledEvent>(LockToggled);

    }
    private void LockToggled(EntityUid uid, EncryptionHolderRequiresLockComponent component, LockToggledEvent args)
    {
        if (!TryComp<LockComponent>(uid, out var lockComp) || !TryComp<EncryptionKeyHolderComponent>(uid, out var keyHolder))
            return;
        keyHolder.KeysUnlocked =  !lockComp.Locked;
        _encryptionKeySystem.UpdateChannels(uid, keyHolder);
    }
}
