using System.Linq;
using Content.Shared.Administration.Logs;
using Content.Shared.CombatMode;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Ghost; // DeltaV - Admin QOL
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Mindshield.Components; // DeltaV - Admin QOL
using Content.Shared.Mobs; // DeltaV - Admin QOL
using Content.Shared.Mobs.Components; // DeltaV - Admin QOL
using Content.Shared.Popups;
using Content.Shared.SSDIndicator; // DeltaV - Admin QOL
using Content.Shared.Strip.Components;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared.Strip;

public abstract class SharedStrippableSystem : EntitySystem
{
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;

    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    [Dependency] private readonly SharedCuffableSystem _cuffableSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;

    private readonly string[] _keyItemSlots = ["id", "belt", "back"]; // DeltaV - high impact on player, key items
    private readonly string[] _extremeStripSlots = ["jumpsuit"]; // DeltaV - people shouldn't be stripping each others clothes off
    private readonly string[] _highSsdStripSlots = ["pocket1", "pocket2"]; // DeltaV - rummaging through pockets of ssd people is not nice

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StrippableComponent, GetVerbsEvent<Verb>>(AddStripVerb);
        SubscribeLocalEvent<StrippableComponent, GetVerbsEvent<ExamineVerb>>(AddStripExamineVerb);

        // BUI
        SubscribeLocalEvent<StrippableComponent, StrippingSlotButtonPressed>(OnStripButtonPressed);

        // DoAfters
        SubscribeLocalEvent<HandsComponent, DoAfterAttemptEvent<StrippableDoAfterEvent>>(OnStrippableDoAfterRunning);
        SubscribeLocalEvent<HandsComponent, StrippableDoAfterEvent>(OnStrippableDoAfterFinished);

        SubscribeLocalEvent<StrippingComponent, CanDropTargetEvent>(OnCanDropOn);
        SubscribeLocalEvent<StrippableComponent, CanDropDraggedEvent>(OnCanDrop);
        SubscribeLocalEvent<StrippableComponent, DragDropDraggedEvent>(OnDragDrop);
        SubscribeLocalEvent<StrippableComponent, ActivateInWorldEvent>(OnActivateInWorld);
    }

    private void AddStripVerb(EntityUid uid, StrippableComponent component, GetVerbsEvent<Verb> args)
    {
        if (args.Hands == null || !args.CanAccess || !args.CanInteract || args.Target == args.User)
            return;

        Verb verb = new()
        {
            Text = Loc.GetString("strip-verb-get-data-text"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/outfit.svg.192dpi.png")),
            Act = () => TryOpenStrippingUi(args.User, (uid, component), true),
        };

        args.Verbs.Add(verb);
    }

    private void AddStripExamineVerb(EntityUid uid, StrippableComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (args.Hands == null || !args.CanAccess || !args.CanInteract || args.Target == args.User)
            return;

        ExamineVerb verb = new()
        {
            Text = Loc.GetString("strip-verb-get-data-text"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/outfit.svg.192dpi.png")),
            Act = () => TryOpenStrippingUi(args.User, (uid, component), true),
            Category = VerbCategory.Examine,
        };

        args.Verbs.Add(verb);
    }

    private void OnStripButtonPressed(Entity<StrippableComponent> strippable, ref StrippingSlotButtonPressed args)
    {
        if (args.Actor is not { Valid: true } user ||
            !TryComp<HandsComponent>(user, out var userHands))
            return;

        if (args.IsHand)
        {
            StripHand((user, userHands), (strippable.Owner, null), args.Slot, strippable);
            return;
        }

        if (!TryComp<InventoryComponent>(strippable, out var inventory))
            return;

        var hasEnt = _inventorySystem.TryGetSlotEntity(strippable, args.Slot, out var held, inventory);

        if (_handsSystem.GetActiveItem((user, userHands)) is { } activeItem && !hasEnt)
            StartStripInsertInventory((user, userHands), strippable.Owner, activeItem, args.Slot);
        else if (hasEnt)
            StartStripRemoveInventory(user, strippable.Owner, held!.Value, args.Slot);
    }

    private void StripHand(
        Entity<HandsComponent?> user,
        Entity<HandsComponent?> target,
        string handId,
        StrippableComponent? targetStrippable)
    {
        if (!Resolve(user, ref user.Comp) ||
            !Resolve(target, ref target.Comp) ||
            !Resolve(target, ref targetStrippable))
            return;

        if (!target.Comp.CanBeStripped)
            return;

        var heldEntity = _handsSystem.GetHeldItem(target.Owner, handId);

        // Is the target a handcuff?
        if (TryComp<VirtualItemComponent>(heldEntity, out var virtualItem) &&
            _cuffableSystem.TryGetAllCuffs(target.Owner, out var cuffs) &&
            cuffs.Contains(virtualItem.BlockingEntity))
        {
            _cuffableSystem.TryUncuff(target.Owner, user, virtualItem.BlockingEntity);
            return;
        }

        if (_handsSystem.GetActiveItem(user.AsNullable()) is { } activeItem && heldEntity == null)
            StartStripInsertHand(user, target, activeItem, handId, targetStrippable);
        else if (heldEntity != null)
            StartStripRemoveHand(user, target, heldEntity.Value, handId, targetStrippable);
    }

    /// <summary>
    ///     Checks whether the item is in a user's active hand and whether it can be inserted into the inventory slot.
    /// </summary>
    private bool CanStripInsertInventory(
        Entity<HandsComponent?> user,
        EntityUid target,
        EntityUid held,
        string slot)
    {
        if (!Resolve(user, ref user.Comp))
            return false;

        if (!_handsSystem.TryGetActiveItem(user, out var activeItem) || activeItem != held)
            return false;

        if (!_handsSystem.CanDropHeld(user, user.Comp.ActiveHandId!))
        {
            _popupSystem.PopupCursor(Loc.GetString("strippable-component-cannot-drop"));
            return false;
        }

        var targetIdentity = Identity.Entity(target, EntityManager);

        if (_inventorySystem.TryGetSlotEntity(target, slot, out _))
        {
            _popupSystem.PopupCursor(Loc.GetString("strippable-component-item-slot-occupied", ("owner", targetIdentity)));
            return false;
        }

        if (!_inventorySystem.CanEquip(user, target, held, slot, out _))
        {
            _popupSystem.PopupCursor(Loc.GetString("strippable-component-cannot-equip-message", ("owner", targetIdentity)));
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Begins a DoAfter to insert the item in the user's active hand into the inventory slot.
    /// </summary>
    private void StartStripInsertInventory(
        Entity<HandsComponent?> user,
        EntityUid target,
        EntityUid held,
        string slot)
    {
        if (!Resolve(user, ref user.Comp))
            return;

        if (!CanStripInsertInventory(user, target, held, slot))
            return;

        if (!_inventorySystem.TryGetSlot(target, slot, out var slotDef))
        {
            Log.Error($"{ToPrettyString(user)} attempted to place an item in a non-existent inventory slot ({slot}) on {ToPrettyString(target)}");
            return;
        }

        var (time, stealth) = GetStripTimeModifiers(user, target, held, slotDef.StripTime);

        if (!stealth)
        {
            _popupSystem.PopupEntity(Loc.GetString("strippable-component-alert-owner-insert",
                                                        ("user", Identity.Entity(user, EntityManager)),
                                                        ("item", _handsSystem.GetActiveItem((user, user.Comp))!.Value)),
                                                        target,
                                                        target,
                                                        PopupType.Large);
        }

        var prefix = stealth ? "stealthily " : "";
        _adminLogger.Add(LogType.Stripping, LogImpact.Low, $"{ToPrettyString(user):actor} is trying to {prefix}place the item {ToPrettyString(held):item} in {ToPrettyString(target):target}'s {slot} slot");

        var doAfterArgs = new DoAfterArgs(EntityManager, user, time, new StrippableDoAfterEvent(true, true, slot), user, target, held)
        {
            Hidden = stealth,
            AttemptFrequency = AttemptFrequency.EveryTick,
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
            DuplicateCondition = DuplicateConditions.SameTool
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    /// <summary>
    ///     Inserts the item in the user's active hand into the inventory slot.
    /// </summary>
    private void StripInsertInventory(
        Entity<HandsComponent?> user,
        EntityUid target,
        EntityUid held,
        string slot)
    {
        if (!Resolve(user, ref user.Comp))
            return;

        if (!CanStripInsertInventory(user, target, held, slot))
            return;

        if (!_handsSystem.TryDrop(user))
            return;

        _inventorySystem.TryEquip(user, target, held, slot, triggerHandContact: true);
        _adminLogger.Add(LogType.Stripping, LogImpact.Medium, $"{ToPrettyString(user):actor} has placed the item {ToPrettyString(held):item} in {ToPrettyString(target):target}'s {slot} slot");
    }

    /// <summary>
    ///     Checks whether the item can be removed from the target's inventory.
    /// </summary>
    private bool CanStripRemoveInventory(
        EntityUid user,
        EntityUid target,
        EntityUid item,
        string slot)
    {
        if (!_inventorySystem.TryGetSlotEntity(target, slot, out var slotItem))
        {
            _popupSystem.PopupCursor(Loc.GetString("strippable-component-item-slot-free-message", ("owner", Identity.Entity(target, EntityManager))));
            return false;
        }

        if (slotItem != item)
            return false;

        if (!_inventorySystem.CanUnequip(user, target, slot, out var reason))
        {
            _popupSystem.PopupCursor(Loc.GetString(reason));
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Begins a DoAfter to remove the item from the target's inventory and insert it in the user's active hand.
    /// </summary>
    private void StartStripRemoveInventory(
        EntityUid user,
        EntityUid target,
        EntityUid item,
        string slot)
    {
        if (!CanStripRemoveInventory(user, target, item, slot))
            return;

        if (!_inventorySystem.TryGetSlot(target, slot, out var slotDef))
        {
            Log.Error($"{ToPrettyString(user)} attempted to take an item from a non-existent inventory slot ({slot}) on {ToPrettyString(target)}");
            return;
        }

        var (time, stealth) = GetStripTimeModifiers(user, target, item, slotDef.StripTime);

        if (!stealth)
        {
            if (IsStripHidden(slotDef, user))
                _popupSystem.PopupEntity(Loc.GetString("strippable-component-alert-owner-hidden", ("slot", slot)), target, target, PopupType.Large);
            else
            {
                _popupSystem.PopupEntity(Loc.GetString("strippable-component-alert-owner",
                                                            ("user", Identity.Entity(user, EntityManager)),
                                                            ("item", item)),
                                                            target,
                                                            target,
                                                            PopupType.Large);

            }
        }

        var prefix = stealth ? "stealthily " : "";
        _adminLogger.Add(LogType.Stripping, LogImpact.Low, $"{ToPrettyString(user):actor} is trying to {prefix}strip the item {ToPrettyString(item):item} from {ToPrettyString(target):target}'s {slot} slot");

        _interactionSystem.DoContactInteraction(user, item);

        var doAfterArgs = new DoAfterArgs(EntityManager, user, time, new StrippableDoAfterEvent(false, true, slot), user, target, item)
        {
            Hidden = stealth,
            AttemptFrequency = AttemptFrequency.EveryTick,
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
            BreakOnHandChange = false, // Allow simultaneously removing multiple items.
            DuplicateCondition = DuplicateConditions.SameTool
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    // DeltaV - Add utility function START
    private (bool isTargetSsd, bool isTargetDead, bool isUserShielded, bool isUserGhost) LogValuesForStripAction(EntityUid user, EntityUid target)
    {
        var isTargetSsd = TryComp<SSDIndicatorComponent>(target, out var ssdIndicator) && ssdIndicator.IsSSD;
        var isTargetDead = TryComp<MobThresholdsComponent>(target, out var thresholds) &&
                           thresholds.CurrentThresholdState == MobState.Dead;
        var isUserShielded = HasComp<MindShieldComponent>(user);
        var isUserGhost = HasComp<GhostComponent>(user);

        return (isTargetSsd, isTargetDead, isUserShielded, isUserGhost);
    }
    // DeltaV - Add utility function END

    /// <summary>
    ///     Removes the item from the target's inventory and inserts it in the user's active hand.
    /// </summary>
    private void StripRemoveInventory(
        EntityUid user,
        EntityUid target,
        EntityUid item,
        string slot,
        bool stealth)
    {
        if (!CanStripRemoveInventory(user, target, item, slot))
            return;

        if (!_inventorySystem.TryUnequip(user, target, slot, triggerHandContact: true))
            return;

        RaiseLocalEvent(item, new DroppedEvent(user), true); // Gas tank internals etc.

        _handsSystem.PickupOrDrop(user, item, animateUser: stealth, animate: !stealth);

        // DeltaV - LogImpact Additions START
        // Previously High by default. Stop chat spam from searches in Sec. Somebody with bad intentions is likely to strip from the specified slots.
        var (isTargetSsd, isTargetDead, isUserShielded, isUserGhost) = LogValuesForStripAction(user, target);
        var logImpact = LogImpact.Medium;

        // If someone strips a key item from a living SSD, always alert. If not SSD or SSD and dead, alert on new player.
        if (_keyItemSlots.Contains(slot.ToLower()))
        {
            logImpact = isTargetSsd && !isTargetDead ? LogImpact.Extreme : LogImpact.High;
        }

        // In any case, alert if a new player is trying to strip certain additional slots from a living SSD
        if (_highSsdStripSlots.Contains(slot.ToLower()) && isTargetSsd && !isTargetDead)
        {
            logImpact = LogImpact.High;
        }

        // ... unless the user is mindshielded. Security searches people who might disconnect.
        if (isUserShielded)
        {
            logImpact = LogImpact.Medium;
        }

        // If someone strips a jumpsuit from a dead player, they're probably trying to perform surgery, alert on new player. Otherwise, always alert, even if shielded.
        if (_extremeStripSlots.Contains(slot.ToLower()))
        {
            logImpact = isTargetDead ? LogImpact.High : LogImpact.Extreme;
        }

        // ... unless the user is an (admin) observer, admins setting up ghost roles in ATAG shouldn't trigger alerts
        if (isUserGhost)
        {
            logImpact = LogImpact.Medium;
        }
        // DeltaV - LogImpact Additions END

        _adminLogger.Add(LogType.Stripping, logImpact, $"{ToPrettyString(user):actor} has stripped the item {ToPrettyString(item):item} from {(isTargetSsd ? "[SSD] " : "")}{(isTargetDead ? "[DEAD] " : "")}{ToPrettyString(target):target}'s {slot} slot"); // DeltaV - replace default LogImpact, insert SSD and DEAD indicators
    }

    /// <summary>
    ///     Checks whether the item in the user's active hand can be inserted into one of the target's hands.
    /// </summary>
    private bool CanStripInsertHand(
        Entity<HandsComponent?> user,
        Entity<HandsComponent?> target,
        EntityUid held,
        string handName)
    {
        if (!Resolve(user, ref user.Comp) ||
            !Resolve(target, ref target.Comp))
            return false;

        if (!target.Comp.CanBeStripped)
            return false;

        if (!_handsSystem.TryGetActiveItem(user, out var activeItem) || activeItem != held)
            return false;

        if (!_handsSystem.CanDropHeld(user, user.Comp.ActiveHandId!))
        {
            _popupSystem.PopupCursor(Loc.GetString("strippable-component-cannot-drop"));
            return false;
        }

        if (!_handsSystem.CanPickupToHand(target, activeItem.Value, handName, checkActionBlocker: false, handsComp: target.Comp))
        {
            _popupSystem.PopupCursor(Loc.GetString("strippable-component-cannot-put-message", ("owner", Identity.Entity(target, EntityManager))));
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Begins a DoAfter to insert the item in the user's active hand into one of the target's hands.
    /// </summary>
    private void StartStripInsertHand(
        Entity<HandsComponent?> user,
        Entity<HandsComponent?> target,
        EntityUid held,
        string handName,
        StrippableComponent? targetStrippable = null)
    {
        if (!Resolve(user, ref user.Comp) ||
            !Resolve(target, ref target.Comp) ||
            !Resolve(target, ref targetStrippable))
            return;

        if (!CanStripInsertHand(user, target, held, handName))
            return;

        var (time, stealth) = GetStripTimeModifiers(user, target, null, targetStrippable.HandStripDelay);

        if (!stealth)
        {
            _popupSystem.PopupEntity(Loc.GetString("strippable-component-alert-owner-insert-hand",
                                                        ("user", Identity.Entity(user, EntityManager)),
                                                        ("item", _handsSystem.GetActiveItem(user)!.Value)),
                                                        target,
                                                        target,
                                                        PopupType.Large);

        }

        var prefix = stealth ? "stealthily " : "";
        _adminLogger.Add(LogType.Stripping, LogImpact.Low, $"{ToPrettyString(user):actor} is trying to {prefix}place the item {ToPrettyString(held):item} in {ToPrettyString(target):target}'s hands");

        var doAfterArgs = new DoAfterArgs(EntityManager, user, time, new StrippableDoAfterEvent(true, false, handName), user, target, held)
        {
            Hidden = stealth,
            AttemptFrequency = AttemptFrequency.EveryTick,
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
            DuplicateCondition = DuplicateConditions.SameTool
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    /// <summary>
    ///     Places the item in the user's active hand into one of the target's hands.
    /// </summary>
    private void StripInsertHand(
        Entity<HandsComponent?> user,
        Entity<HandsComponent?> target,
        EntityUid held,
        string handName,
        bool stealth)
    {
        if (!Resolve(user, ref user.Comp) ||
            !Resolve(target, ref target.Comp))
            return;

        if (!CanStripInsertHand(user, target, held, handName))
            return;

        _handsSystem.TryDrop(user, checkActionBlocker: false);
        _handsSystem.TryPickup(target, held, handName, checkActionBlocker: false, animateUser: stealth, animate: !stealth, handsComp: target.Comp);
        _adminLogger.Add(LogType.Stripping, LogImpact.Medium, $"{ToPrettyString(user):actor} has placed the item {ToPrettyString(held):item} in {ToPrettyString(target):target}'s hands");

        // Hand update will trigger strippable update.
    }

    /// <summary>
    ///     Checks whether the item is in the target's hand and whether it can be dropped.
    /// </summary>
    private bool CanStripRemoveHand(
        EntityUid user,
        Entity<HandsComponent?> target,
        EntityUid item,
        string handName)
    {
        if (!Resolve(target, ref target.Comp))
            return false;

        if (!target.Comp.CanBeStripped)
            return false;

        if (!_handsSystem.TryGetHand(target, handName, out _))
        {
            _popupSystem.PopupCursor(Loc.GetString("strippable-component-item-slot-free-message", ("owner", Identity.Entity(target, EntityManager))));
            return false;
        }

        if (!_handsSystem.TryGetHeldItem(target, handName, out var heldEntity))
            return false;

        if (HasComp<VirtualItemComponent>(heldEntity))
            return false;

        if (heldEntity != item)
            return false;

        if (!_handsSystem.CanDropHeld(target, handName, false))
        {
            _popupSystem.PopupCursor(Loc.GetString("strippable-component-cannot-drop-message", ("owner", Identity.Entity(target, EntityManager))));
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Begins a DoAfter to remove the item from the target's hand and insert it in the user's active hand.
    /// </summary>
    private void StartStripRemoveHand(
        Entity<HandsComponent?> user,
        Entity<HandsComponent?> target,
        EntityUid item,
        string handName,
        StrippableComponent? targetStrippable = null)
    {
        if (!Resolve(user, ref user.Comp) ||
            !Resolve(target, ref target.Comp) ||
            !Resolve(target, ref targetStrippable))
            return;

        if (!CanStripRemoveHand(user, target, item, handName))
            return;

        var (time, stealth) = GetStripTimeModifiers(user, target, null, targetStrippable.HandStripDelay);

        if (!stealth)
        {
            _popupSystem.PopupEntity(Loc.GetString("strippable-component-alert-owner",
                                                        ("user", Identity.Entity(user, EntityManager)),
                                                        ("item", item)),
                                                        target,
                                                        target);
        }

        var prefix = stealth ? "stealthily " : "";
        _adminLogger.Add(LogType.Stripping, LogImpact.Low, $"{ToPrettyString(user):actor} is trying to {prefix}strip the item {ToPrettyString(item):item} from {ToPrettyString(target):target}'s hands");

        _interactionSystem.DoContactInteraction(user, item);

        var doAfterArgs = new DoAfterArgs(EntityManager, user, time, new StrippableDoAfterEvent(false, false, handName), user, target, item)
        {
            Hidden = stealth,
            AttemptFrequency = AttemptFrequency.EveryTick,
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
            BreakOnHandChange = false, // Allow simultaneously removing multiple items.
            DuplicateCondition = DuplicateConditions.SameTool
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    /// <summary>
    ///     Takes the item from the target's hand and inserts it in the user's active hand.
    /// </summary>
    private void StripRemoveHand(
        Entity<HandsComponent?> user,
        Entity<HandsComponent?> target,
        EntityUid item,
        string handName,
        bool stealth)
    {
        if (!Resolve(user, ref user.Comp) ||
            !Resolve(target, ref target.Comp))
            return;

        if (!CanStripRemoveHand(user, target, item, handName))
            return;

        _handsSystem.TryDrop(target, item, checkActionBlocker: false);
        _handsSystem.PickupOrDrop(user, item, animateUser: stealth, animate: !stealth, handsComp: user.Comp);

        var (isTargetSsd, isTargetDead, isUserShielded, isUserGhost) = LogValuesForStripAction(user, target); // DeltaV

        // DeltaV - Add conditional logImpact START
        // Lower default to Medium - the target is much more likely to notice, the item was probably being offered.
        var logImpact = LogImpact.Medium;

        // If they are SSD
        if (isTargetSsd)
        {
            // ... and dead, alert on new player.
            if (isTargetDead)
            {
                logImpact = LogImpact.High;
            }
            // TODO: ... and living, and the user is not mindshielded, alert on recent SSD
            // Previously this was setting it to Extreme always if not shielded, but that's going to lead to a lot of false positives.
            // Set it to extreme again once SSD Recency is merged and it happened in the first stage.
            else if (!isUserShielded)
            {
                // logImpact = LogImpact.Extreme;
                logImpact = LogImpact.High;
            }
        }

        // If the user is an (admin) observer, don't alert.
        if (isUserGhost)
        {
            logImpact = LogImpact.Medium;
        }
        // DeltaV - Add conditional logImpact END

        _adminLogger.Add(LogType.Stripping, logImpact, $"{ToPrettyString(user):actor} has stripped the item {ToPrettyString(item):item} from {(isTargetSsd ? "[SSD] " : "")}{(isTargetDead ? "[DEAD] " : "")}{ToPrettyString(target):target}'s hands"); // DeltaV - replace logImpact, previously High. add SSD and DEAD indicators.
        // Hand update will trigger strippable update.
    }

    private void OnStrippableDoAfterRunning(Entity<HandsComponent> entity, ref DoAfterAttemptEvent<StrippableDoAfterEvent> ev)
    {
        var args = ev.DoAfter.Args;

        DebugTools.Assert(entity.Owner == args.User);
        DebugTools.Assert(args.Target != null);
        DebugTools.Assert(args.Used != null);
        DebugTools.Assert(ev.Event.SlotOrHandName != null);

        if (ev.Event.InventoryOrHand)
        {
            if ( ev.Event.InsertOrRemove && !CanStripInsertInventory((entity.Owner, entity.Comp), args.Target.Value, args.Used.Value, ev.Event.SlotOrHandName) ||
                !ev.Event.InsertOrRemove && !CanStripRemoveInventory(entity.Owner, args.Target.Value, args.Used.Value, ev.Event.SlotOrHandName))
            {
                ev.Cancel();
            }
        }
        else
        {
            if ( ev.Event.InsertOrRemove && !CanStripInsertHand((entity.Owner, entity.Comp), args.Target.Value, args.Used.Value, ev.Event.SlotOrHandName) ||
                !ev.Event.InsertOrRemove && !CanStripRemoveHand(entity.Owner, args.Target.Value, args.Used.Value, ev.Event.SlotOrHandName))
            {
                ev.Cancel();
            }
        }
    }

    private void OnStrippableDoAfterFinished(Entity<HandsComponent> entity, ref StrippableDoAfterEvent ev)
    {
        if (ev.Cancelled)
            return;

        DebugTools.Assert(entity.Owner == ev.User);
        DebugTools.Assert(ev.Target != null);
        DebugTools.Assert(ev.Used != null);
        DebugTools.Assert(ev.SlotOrHandName != null);

        if (ev.InventoryOrHand)
        {
            if (ev.InsertOrRemove)
                StripInsertInventory((entity.Owner, entity.Comp), ev.Target.Value, ev.Used.Value, ev.SlotOrHandName);
            else
                StripRemoveInventory(entity.Owner, ev.Target.Value, ev.Used.Value, ev.SlotOrHandName, ev.Args.Hidden);
        }
        else
        {
            if (ev.InsertOrRemove)
                StripInsertHand((entity.Owner, entity.Comp), ev.Target.Value, ev.Used.Value, ev.SlotOrHandName, ev.Args.Hidden);
            else
                StripRemoveHand((entity.Owner, entity.Comp), ev.Target.Value, ev.Used.Value, ev.SlotOrHandName, ev.Args.Hidden);
        }
    }

    private void OnActivateInWorld(EntityUid uid, StrippableComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex || args.Target == args.User)
            return;

        if (TryOpenStrippingUi(args.User, (uid, component)))
            args.Handled = true;
    }

    /// <summary>
    /// Modify the strip time via events. Raised directed at the item being stripped, the player stripping someone and the player being stripped.
    /// </summary>
    public (TimeSpan Time, bool Stealth) GetStripTimeModifiers(EntityUid user, EntityUid targetPlayer, EntityUid? targetItem, TimeSpan initialTime)
    {
        var itemEv = new BeforeItemStrippedEvent(initialTime, false);
        if (targetItem != null)
            RaiseLocalEvent(targetItem.Value, ref itemEv);
        var userEv = new BeforeStripEvent(itemEv.Time, itemEv.Stealth);
        RaiseLocalEvent(user, ref userEv);
        var targetEv = new BeforeGettingStrippedEvent(userEv.Time, userEv.Stealth);
        RaiseLocalEvent(targetPlayer, ref targetEv);
        return (targetEv.Time, targetEv.Stealth);
    }

    private void OnDragDrop(EntityUid uid, StrippableComponent component, ref DragDropDraggedEvent args)
    {
        // If the user drags a strippable thing onto themselves.
        if (args.Handled || args.Target != args.User)
            return;

        if (TryOpenStrippingUi(args.User, (uid, component)))
            args.Handled = true;
    }

    public bool TryOpenStrippingUi(EntityUid user, Entity<StrippableComponent> target, bool openInCombat = false)
    {
        if (!openInCombat && TryComp<CombatModeComponent>(user, out var mode) && mode.IsInCombatMode)
            return false;

        if (!HasComp<StrippingComponent>(user))
            return false;

        _ui.OpenUi(target.Owner, StrippingUiKey.Key, user);
        return true;
    }

    private void OnCanDropOn(EntityUid uid, StrippingComponent component, ref CanDropTargetEvent args)
    {
        var val = uid == args.User &&
                  HasComp<StrippableComponent>(args.Dragged) &&
                  HasComp<HandsComponent>(args.User) &&
                  HasComp<StrippingComponent>(args.User);
        args.Handled |= val;
        args.CanDrop |= val;
    }

    private void OnCanDrop(EntityUid uid, StrippableComponent component, ref CanDropDraggedEvent args)
    {
        args.CanDrop |= args.Target == args.User &&
                        HasComp<StrippingComponent>(args.User) &&
                        HasComp<HandsComponent>(args.User);

        if (args.CanDrop)
            args.Handled = true;
    }

    public bool IsStripHidden(SlotDefinition definition, EntityUid? viewer)
    {
        if (!definition.StripHidden)
            return false;

        if (viewer == null)
            return true;

        return !HasComp<BypassInteractionChecksComponent>(viewer);
    }
}
