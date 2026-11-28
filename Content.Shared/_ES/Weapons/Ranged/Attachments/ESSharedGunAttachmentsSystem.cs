using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._DV.Weapons.Ranged.Components; // DeltaV
using Content.Shared._ES.Weapons.Ranged.Attachments.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Localizations;
using Content.Shared.Weapons.Ranged.Components; // DeltaV
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Whitelist;
using Content.Shared.Wieldable; // DeltaV
using Content.Shared.Wieldable.Components; // DeltaV
using Robust.Shared.Containers;
using Robust.Shared.Serialization.Manager; // Mono
using Robust.Shared.Timing; // Mono
using Robust.Shared.Utility;

namespace Content.Shared._ES.Weapons.Ranged.Attachments;

public abstract class ESSharedGunAttachmentsSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!; // Mono
    [Dependency] private readonly IGameTiming _timing = default!; // Mono
    [Dependency] private readonly SharedWieldableSystem _wieldable = default!; // DeltaV

    private EntityQuery<ESGunAttachmentComponent> _attachmentQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ESAttachableGunComponent, EntInsertedIntoContainerMessage>(OnEntInsertedIntoContainer);
        SubscribeLocalEvent<ESAttachableGunComponent, EntRemovedFromContainerMessage>(OnEntRemovedFromContainer);
        SubscribeLocalEvent<ESAttachableGunComponent, GunRefreshModifiersEvent>(OnGunRefreshModifiers, after: [typeof(SharedWieldableSystem)]); // DeltaV - after SharedWieldableSystem
        SubscribeLocalEvent<ESAttachableGunComponent, InteractUsingEvent>(OnAfterInteract, before: [typeof(ItemSlotsSystem)]);
        SubscribeLocalEvent<ESAttachableGunComponent, ExaminedEvent>(OnExamined);

        SubscribeAllEvent<ESAttachableGunModifySlotEvent>(OnAttachableGunModifySlots);

        SubscribeLocalEvent<ESGunSoundAttachmentComponent, GunRefreshModifiersEvent>(OnGunSoundRefreshModifiers, after: [typeof(SharedWieldableSystem)]); // DeltaV - after SharedWieldableSystem
        SubscribeLocalEvent<ESGunRecoilAttachmentComponent, GunRefreshModifiersEvent>(OnGunRecoilRefreshModifiers, after: [typeof(SharedWieldableSystem)]); // Mono // DeltaV - after SharedWieldableSystem

        SubscribeLocalEvent<ESGunComponentAttachmentComponent, GunRefreshModifiersEvent>(OnCompAttachmentEquip, after: [typeof(SharedWieldableSystem)]); // Mono // DeltaV - after SharedWieldableSystem
        SubscribeLocalEvent<ESGunComponentAttachmentComponent, EntGotRemovedFromContainerMessage>(OnCompAttachmentUnequip); // Mono
        SubscribeLocalEvent<ESGunRecoilAttachmentComponent, ExaminedEvent>(OnAttachmentExamined); // Mono

        SubscribeLocalEvent<ESAttachableGunComponent, AmmoShotEvent>(OnAmmoShot); // DeltaV

        _attachmentQuery = GetEntityQuery<ESGunAttachmentComponent>();
    }

    protected virtual void OnEntInsertedIntoContainer(Entity<ESAttachableGunComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        var containerId = args.Container.ID;
        if (!ent.Comp.Slots.Any(s => s.ContainerId.Equals(containerId)))
            return;
        _gun.RefreshModifiers(ent.Owner);
    }

    protected virtual void OnEntRemovedFromContainer(Entity<ESAttachableGunComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        var containerId = args.Container.ID;
        if (!ent.Comp.Slots.Any(s => s.ContainerId.Equals(containerId)))
            return;
        _gun.RefreshModifiers(ent.Owner);
    }

    private void OnGunRefreshModifiers(Entity<ESAttachableGunComponent> ent, ref GunRefreshModifiersEvent args)
    {
        foreach (var attachment in EnumerateAttachments(ent))
        {
            RaiseLocalEvent(attachment, ref args);
        }
    }

    private void OnAfterInteract(Entity<ESAttachableGunComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryFindEmptyValidSlot(ent.AsNullable(), args.Used, out var slot))
            return;
        args.Handled = TryInsertAttachment(ent.AsNullable(), args.Used, slot.Value);
    }

    private void OnExamined(Entity<ESAttachableGunComponent> ent, ref ExaminedEvent args)
    {
        var attachments = EnumerateAttachments(ent).ToList();
        if (attachments.Count == 0)
            return;

        var attachmentString = ContentLocalizationManager.FormatList(attachments.Select(e => Name(e)).ToList());
        args.PushMarkup(Loc.GetString("es-gun-attachment-examine-text", ("attachments", attachmentString)));
    }

    private void OnAttachableGunModifySlots(ESAttachableGunModifySlotEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user ||
            !TryGetEntity(msg.Gun, out var gunUid) ||
            !TryComp<ESAttachableGunComponent>(gunUid, out var gunComp) ||
            !gunComp.Slots.TryGetValue(msg.SlotIndex, out var slot))
            return;

        if (!_actionBlocker.CanInteract(user, gunUid))
            return;

        if (TryGetAttachment((gunUid.Value, gunComp), slot, out var attachment))
        {
            _container.Remove(attachment.Value.Owner, _container.GetContainer(gunUid.Value, slot.ContainerId));
            _hands.TryPickupAnyHand(user, attachment.Value);
        }
        else if (_hands.TryGetActiveItem(user, out var held))
        {
            TryInsertAttachment((gunUid.Value, gunComp), held.Value, slot);
        }
    }

    private void OnGunSoundRefreshModifiers(Entity<ESGunSoundAttachmentComponent> ent, ref GunRefreshModifiersEvent args)
    {
        args.SoundGunshot = ent.Comp.Sound;
    }

    // Mono start
    private void OnGunRecoilRefreshModifiers(Entity<ESGunRecoilAttachmentComponent> ent, ref GunRefreshModifiersEvent args)
    {
        args.AngleIncrease = (args.AngleIncrease * ent.Comp.RecoilIncreaseModifier);
        args.AngleDecay = (args.AngleDecay * ent.Comp.RecoilRecoveryModifier);

        args.MinAngle = (args.MinAngle * ent.Comp.MinSpreadModifier);
        args.MaxAngle = (args.MaxAngle * ent.Comp.MaxSpreadModifier);
    }
    // Mono end

    public bool HasAttachment(Entity<ESAttachableGunComponent> ent, ESGunAttachmentSlot slot)
    {
        return TryGetAttachment(ent, slot, out _);
    }

    public bool TryGetAttachment(Entity<ESAttachableGunComponent> ent, ESGunAttachmentSlot slot, [NotNullWhen(true)] out Entity<ESGunAttachmentComponent>? attachment)
    {
        attachment = null;
        if (!_container.TryGetContainer(ent, slot.ContainerId, out var container))
            return false;

        foreach (var contained in container.ContainedEntities)
        {
            if (!IsAttachmentValid(contained, slot))
                continue;
            attachment = (contained, _attachmentQuery.Get(contained));
            return true;
        }
        return false;
    }

    public bool IsAttachmentValid(Entity<ESGunAttachmentComponent?> ent, ESGunAttachmentSlot slot)
    {
        if (!_attachmentQuery.Resolve(ent, ref ent.Comp))
            return false;

        return _entityWhitelist.IsWhitelistPass(slot.Whitelist, ent);
    }

    public bool TryFindEmptyValidSlot(Entity<ESAttachableGunComponent?> gun,
        Entity<ESGunAttachmentComponent?> attachment,
        [NotNullWhen(true)] out ESGunAttachmentSlot? outSlot)
    {
        outSlot = null;
        if (!Resolve(gun, ref gun.Comp) ||
            !Resolve(attachment, ref attachment.Comp, false))
            return false;

        foreach (var slot in gun.Comp.Slots)
        {
            // Slot is filled, can't be used.
            if (HasAttachment((gun, gun.Comp), slot))
                continue;

            if (!IsAttachmentValid(attachment, slot))
                continue;
            outSlot = slot;
            break;
        }

        return outSlot != null;
    }

    public bool TryInsertAttachment(Entity<ESAttachableGunComponent?> gun, Entity<ESGunAttachmentComponent?> attachment, ESGunAttachmentSlot slot)
    {
        if (!Resolve(gun, ref gun.Comp) ||
            !Resolve(attachment, ref attachment.Comp, false))
            return false;

        if (HasAttachment((gun, gun.Comp), slot) || !IsAttachmentValid(attachment, slot))
            return false;

        InsertAttachment(gun, attachment, slot);
        return true;
    }

    public void InsertAttachment(Entity<ESAttachableGunComponent?> gun, Entity<ESGunAttachmentComponent?> attachment, ESGunAttachmentSlot slot)
    {
        if (!Resolve(gun, ref gun.Comp) ||
            !Resolve(attachment, ref attachment.Comp))
            return;

        var container = _container.GetContainer(gun, slot.ContainerId);
        _container.Insert(attachment.Owner, container);
    }

    public IEnumerable<Entity<ESGunAttachmentComponent>> EnumerateAttachments(Entity<ESAttachableGunComponent> ent)
    {
        foreach (var slot in ent.Comp.Slots)
        {
            if (TryGetAttachment(ent, slot, out var attachment))
                yield return attachment.Value;
        }
    }

    // Mono start
    private void OnCompAttachmentEquip(EntityUid uid, ESGunComponentAttachmentComponent component, GunRefreshModifiersEvent args)
    {
        if (_timing.ApplyingState)
            return;
        var target = args.Gun.Owner;
        EntityManager.AddComponents(target, component.Components, removeExisting: false); // DeltaV - removeExisting: false, fixes more bugs then it causes (I hope)
    }

    private void OnCompAttachmentUnequip(EntityUid uid, ESGunComponentAttachmentComponent component, EntGotRemovedFromContainerMessage args)
    {
        var target = args.Container.Owner;

        // DeltaV - ONLY clean up gun attachment slots
        if (!TryComp<ESAttachableGunComponent>(target, out var gun) || gun.Slots.All(slot => slot.ContainerId != args.Container.ID))
            return;

        // Begin DeltaV - evil ass fixes
        // evil ass wieldable jank fix (lasers, stocks)
        if (TryComp<WieldableComponent>(target, out var wieldable) && wieldable.Wielded && TryComp<GunComponent>(target, out var gunComp) &&
            gunComp.Holder is { } holder)
        {
            _wieldable.TryUnwield(target, wieldable, holder, force: true);
        }

        // evil ass bipod jank fix (bipod)
        if (TryComp<GunBipodComponent>(target, out var bipod) &&
            bipod.IsSetup &&
            TryComp<GunComponent>(target, out var gunComponent) &&
            gunComponent.Holder is { } bipodUser)
        {
            _gun.PackUpBipod((target, bipod), bipodUser, null);
        }
        // End DeltaV - evil ass fixes

        EntityManager.RemoveComponents(target, component.Components);
    }

    private void OnAttachmentExamined(EntityUid uid, ESGunRecoilAttachmentComponent component, ExaminedEvent args)
    {
        // only the most beautiful code here. u mad?
        TryComp<ESGunRecoilAttachmentComponent>(uid, out var recoilComponent);
        if (recoilComponent != null)
        {
            Color GetColor(float m) => m > 1 ? Color.Crimson : m < 1 ? Color.Lime : Color.Gold;

            var recoilRecovery = recoilComponent.RecoilRecoveryModifier;
            var recoilRecoveryColor = GetColor((1f / recoilRecovery));

            var recoilIncrease = recoilComponent.RecoilIncreaseModifier;
            var recoilIncreaseColor = GetColor(recoilIncrease);

            var minSpread = recoilComponent.MinSpreadModifier;
            var minSpreadColor = GetColor(minSpread);

            var maxSpread = recoilComponent.MaxSpreadModifier;
            var maxSpreadColor = GetColor(maxSpread);

        // welcome to The Monolith.... I am lazy....
            args.PushMarkup(Loc.GetString("es-gun-attachments-inspect-modifier-recovery",("color", recoilRecoveryColor),("modifier", recoilRecovery)));
            args.PushMarkup(Loc.GetString("es-gun-attachments-inspect-modifier-recoil",("color", recoilIncreaseColor),("modifier", recoilIncrease)));
            args.PushMarkup(Loc.GetString("es-gun-attachments-inspect-modifier-minspread",("color", minSpreadColor),("modifier", minSpread)));
            args.PushMarkup(Loc.GetString("es-gun-attachments-inspect-modifier-maxspread",("color", maxSpreadColor),("modifier", maxSpread)));
        }
    }
    // Mono end

    // Begin DeltaV - AmmoShot fwd
    private void OnAmmoShot(Entity<ESAttachableGunComponent> ent, ref AmmoShotEvent args)
    {
        foreach (var attachment in EnumerateAttachments(ent))
        {
            RaiseLocalEvent(attachment.Owner, args);
        }
    }
    // End DeltaV
}
