using Content.Shared._DV.Carrying;
using Content.Shared.Alert;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

// Dear contributor.
// This system is fucking unmaintainable.
// If you ever happen to touch this again, please do your best to document your changes and try to resolve mysteries surrounding this code.
// I did what I could to document the parts I managed to understand, but there is still more truth to be unveiled.
//
// HOURS_WASTED_HERE_FLOOFSTATION = 10
// HOURS_WASTED_HERE_DELTAV = 1

namespace Content.Shared._Floof.OfferItem;

public abstract partial class SharedOfferItemSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly CarryingSystem _carrying = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OfferItemComponent, AcceptOfferAlertEvent>(OnAcceptOffer);
        SubscribeLocalEvent<OfferItemComponent, InteractUsingEvent>(OnInteractWithReceiver, before: [typeof(IngestionSystem)]);
        SubscribeLocalEvent<OfferableVirtualItemComponent, BeforeRangedInteractEvent>(OnRangedInteractWithReceiver);
        SubscribeLocalEvent<OfferItemComponent, MoveEvent>(OnMove);

        SubscribeLocalEvent<BeingCarriedComponent, ItemTransferredEvent>(OnCarryTransfer);
        SubscribeLocalEvent<PullableComponent, ItemTransferredEvent>(OnPulledTransfer);

        InitializeInteractions();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<OfferItemComponent, HandsComponent>();
        while (query.MoveNext(out var uid, out var offerItem, out var hands))
        {
            // If the mob no longer holds an item in the original offering hand, clear offering mode
            if (offerItem.Hand != null && !_hands.TryGetHeldItem((uid, hands), offerItem.Hand, out _))
            {
                if (offerItem.ReceivingFrom != null)
                {
                    UnReceive(offerItem.ReceivingFrom.Value, offererComp: offerItem);
                    offerItem.IsInOfferMode = false;
                    Dirty(uid, offerItem);
                }
                else
                    UnOffer(uid, offerItem);
            }

            if (!offerItem.IsInReceiveMode)
            {
                _alertsSystem.ClearAlert(uid, offerItem.OfferAlert);
                continue;
            }

            _alertsSystem.ShowAlert(uid, offerItem.OfferAlert);
        }
    }

    #region Events
    private void OnAcceptOffer(Entity<OfferItemComponent> ent, ref AcceptOfferAlertEvent args)
    {
        Receive((ent, ent.Comp));
    }

    private void OnInteractWithReceiver(Entity<OfferItemComponent> receiver, ref InteractUsingEvent args)
    {
        if (!_timing.IsFirstTimePredicted || _timing.ApplyingState || args.Handled)
            return;

        if (!TryComp<OfferItemComponent>(args.User, out var offererComponent))
            return;

        args.Handled = CreateOffer(receiver, (args.User, offererComponent));
    }

    private void OnRangedInteractWithReceiver(Entity<OfferableVirtualItemComponent> virtItem, ref BeforeRangedInteractEvent args)
    {
        // If the entity being offered is a virtual item, InteractUsing will not be raised
        // because virtual items exclude themselves from being marked as used
        // If this is the case, InteractHand will be raised instead, which we can use anyway because OfferItem.Item stores the offered item
        //
        // We also can't check Handled here because VirtualItemSystem handles it, ffs
        // This won't lead you to accidentally offering someone your gun
        //
        // This is shitcode, this time my shitcode. My changes to the offering system allow you to transfer carrying and pulling,
        // but in order to handle these, we need to be able to intercept interactions with virtual items.
        //
        // Ideally this code should be rewritten to:
        // a) Have each different virtual item have a distinct component (e.g. CarryingVirtualItem) which would allow to distinguish them from the rest
        // b) Not rely on the InteractionSystem.
        // However, I'm not in the mood to do either. And I'm too deep into the rabbit hole of getting this shit to work.
        if (!_timing.IsFirstTimePredicted || _timing.ApplyingState)
            return;

        var receiver = args.Target;
        if (!TryComp<OfferItemComponent>(receiver, out var receiverComponent))
            return;

        var offerer = args.User;
        if (!TryComp<OfferItemComponent>(offerer, out var offererComponent) || offererComponent.Item == null)
            return;

        // Since this is ranged, we must also check distance, because the interaction system wont check it for us in this case
        if (!Transform(offerer).Coordinates.TryDistance(EntityManager, _transform, Transform(receiver.Value).Coordinates, out var dst)
            || dst > offererComponent.MaxOfferDistance)
            return;

        args.Handled = CreateOffer((receiver.Value, receiverComponent), (offerer, offererComponent));
    }

    private void OnMove(EntityUid uid, OfferItemComponent component, MoveEvent args)
    {
        if (_net.IsClient) // Client often mispredicts movement, we cant trust it here
            return;

        if (component.ReceivingFrom == null)
            return;

        if (_transform.InRange(args.NewPosition, Transform(component.ReceivingFrom.Value).Coordinates, component.MaxOfferDistance))
            return;

        UnOffer(uid, component);
    }

    private void OnCarryTransfer(Entity<BeingCarriedComponent> ent, ref ItemTransferredEvent args)
    {
        if (args.Handled
            || args.PassedItem == args.RealItem // Means the entity is transferred NOT via carrying
            || args.RealItem is not { Valid: true } carried
            || ent.Comp.Carrier is not { Valid: true } oldCarrier)
            return;

        _carrying.DropCarried(oldCarrier, ent);
        args.Handled = _carrying.TryCarry(args.Target, carried);
    }

    private void OnPulledTransfer(Entity<PullableComponent> ent, ref ItemTransferredEvent args)
    {
        if (args.Handled
            || args.PassedItem == args.RealItem // Means the entity is transferred NOT via pulling
            || args.RealItem is not { Valid: true } pulled)
            return;

        _pulling.TryStopPull(pulled, ent);
        args.Handled = _pulling.TryStartPull(args.Target, ent, null, ent.Comp);
    }

    #endregion

    #region Offering / Recieving
    /// <summary>
    ///     Attempts to create an offer. Expects offerer.Item to already be set to the offered item, offererComponent.InReceiveMode == true.
    ///     Will fail if offerer == receiver or if receiver already has a set TargetOrOfferer, and that person is not the current offerer
    /// </summary>
    private bool CreateOffer(Entity<OfferItemComponent> receiver, Entity<OfferItemComponent> offerer)
    {
        var offererComponent = offerer.Comp;
        var receiverComponent = receiver.Comp;
        if (offerer == receiver || receiverComponent.IsInReceiveMode || !offererComponent.IsInOfferMode)
            return false;

        if (offererComponent.IsInReceiveMode && offererComponent.ReceivingFrom != receiver)
            return false;

        receiverComponent.IsInReceiveMode = true;
        receiverComponent.ReceivingFrom = offerer;

        Dirty(receiver, receiverComponent);

        offererComponent.ReceivingFrom = receiver; // TODO this is ee shitcode, may not be necessary?
        offererComponent.IsInOfferMode = false;

        Dirty(offerer, offererComponent);

        if (offererComponent.Item == null)
            return false;

        // Sender popup (client-side only)
        _popup.PopupClient(
            Loc.GetString("offer-item-try-give",
                ("item", Identity.Entity(offererComponent.GetRealEntity(EntityManager), EntityManager)),
                ("target", Identity.Entity(receiver, EntityManager))),
            offerer,
            offerer);
        // Receiver popup (server side only, not predicted because recipient != local player)
        _popup.PopupEntity(
            Loc.GetString("offer-item-try-give-target",
                ("user", Identity.Entity(receiverComponent.ReceivingFrom.Value, EntityManager)),
                ("item", Identity.Entity(offererComponent.GetRealEntity(EntityManager), EntityManager))),
            offerer,
            receiver,
            Popups.PopupType.Medium);

        return true;
    }



    /// <summary>
    /// Resets the <see cref="OfferItemComponent"/> of the user and the target
    /// </summary>
    protected void UnOffer(EntityUid thisEntity, OfferItemComponent offererComp)
    {
        if (!TryComp<HandsComponent>(thisEntity, out var hands) || _hands.GetActiveHand((thisEntity, hands)) is null)
            return;

        if (offererComp.ReceivingFrom is { } otherEntity && TryComp<OfferItemComponent>(otherEntity, out var otherOfferer))
        {
            // So this tries to figure out which of these entities do what...
            // if A.OfferItemComponent.Item != null, then A is currently offering an item to A.OfferItemComponent.TargetOrOfferer
            // If it is null, then it is ONLY being offered an item TO.
            if (offererComp.Item != null && _net.IsServer)
            {
                _popup.PopupEntity(
                    Loc.GetString("offer-item-no-give",
                        ("item", Identity.Entity(offererComp.GetRealEntity(EntityManager), EntityManager)), // Floof - resolve virtual items
                        ("target", Identity.Entity(otherEntity, EntityManager))),
                    thisEntity,
                    thisEntity);
                _popup.PopupEntity(
                    Loc.GetString("offer-item-no-give-target",
                        ("user", Identity.Entity(thisEntity, EntityManager)),
                        ("item", Identity.Entity(offererComp.GetRealEntity(EntityManager), EntityManager))),
                    thisEntity,
                    otherEntity);
            }

            else if (otherOfferer.Item != null && _net.IsServer)
            {
                _popup.PopupEntity(
                    Loc.GetString("offer-item-no-give",
                        ("item", Identity.Entity(otherOfferer.GetRealEntity(EntityManager), EntityManager)), // Floof - resolve virtual items
                        ("target", Identity.Entity(thisEntity, EntityManager))),
                    otherEntity,
                    otherEntity);
                _popup.PopupEntity(
                    Loc.GetString("offer-item-no-give-target",
                        ("user", Identity.Entity(otherEntity, EntityManager)),
                        ("item", Identity.Entity(otherOfferer.GetRealEntity(EntityManager), EntityManager))),
                    otherEntity,
                    thisEntity);
            }

            otherOfferer.IsInOfferMode = false;
            otherOfferer.IsInReceiveMode = false;
            otherOfferer.Hand = null;
            otherOfferer.ReceivingFrom = null;
            otherOfferer.Item = null;

            Dirty(otherEntity, otherOfferer);
        }

        offererComp.IsInOfferMode = false;
        offererComp.IsInReceiveMode = false;
        offererComp.Hand = null;
        offererComp.ReceivingFrom = null;
        offererComp.Item = null;

        Dirty(thisEntity, offererComp);
    }


    /// <summary>
    /// Cancels the transfer of the item
    /// </summary>
    protected void UnReceive(EntityUid receiver, OfferItemComponent? receiverComp = null, OfferItemComponent? offererComp = null)
    {
        if (!Resolve(receiver, ref receiverComp)
            || receiverComp.ReceivingFrom is not {} offerer
            || !Resolve(offerer, ref offererComp))
            return;

        // Idk why this check is here
        if (!TryComp<HandsComponent>(receiver, out var hands) || _hands.GetActiveHand((receiver, hands)) == null || receiverComp.ReceivingFrom == null)
            return;

        // If offererComp.Item != null, then they are actively offering to TargetOrOfferer
        // Normally this method is called right after a transfer is done, but this part can be called from SetInOfferMode when the player presses F again to cancel an ongoing offer
        if (offererComp.Item != null)
        {
            _popup.PopupClient(
                Loc.GetString("offer-item-no-give",
                    ("item", Identity.Entity(offererComp.GetRealEntity(EntityManager), EntityManager)), // Floof - resolve virtual items
                    ("target", Identity.Entity(receiver, EntityManager))),
                offerer,
                offerer);
            _popup.PopupEntity(
                Loc.GetString("offer-item-no-give-target",
                    ("user", Identity.Entity(receiverComp.ReceivingFrom.Value, EntityManager)), // Floof - resolve virtual items
                    ("item", Identity.Entity(offererComp.GetRealEntity(EntityManager), EntityManager))),
                offerer,
                receiver);
        }

        if (!offererComp.IsInReceiveMode)
        {
            offererComp.ReceivingFrom = null;
            receiverComp.ReceivingFrom = null;
        }

        offererComp.Item = null;
        offererComp.Hand = null;
        receiverComp.IsInReceiveMode = false;

        Dirty(receiver, receiverComp);
    }

    /// <summary>
    /// Accepting the offer and receive item
    /// </summary>
    public void Receive(Entity<OfferItemComponent?> receiver)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (!Resolve(receiver, ref receiver.Comp))
            return;

        if (!TryComp<OfferItemComponent>(receiver.Comp.ReceivingFrom, out var offererComponent) ||
            offererComponent.Hand == null ||
            receiver.Comp.ReceivingFrom is not {} sender ||
            !TryComp<HandsComponent>(receiver, out var hands))
            return;

        if (offererComponent.Item != null)
        {
            // Floof - check if there's something else handling it first
            var realItem = offererComponent.GetRealEntity(EntityManager);
            if (!TryHandleExtendedTransfer(sender, receiver, offererComponent.Item.Value, realItem)
                && !_hands.TryPickup(receiver, offererComponent.Item.Value, handsComp: hands))
            {
                _popup.PopupEntity(Loc.GetString("offer-item-full-hand"), receiver, receiver);
                return;
            }

            _popup.PopupEntity(
                Loc.GetString("offer-item-give",
                    ("item", Identity.Entity(realItem, EntityManager)), // FLoof - resolve virtual items
                    ("target", Identity.Entity(receiver, EntityManager))),
                sender,
                sender);
            _popup.PopupEntity(
                Loc.GetString("offer-item-give-other",
                    ("user", Identity.Entity(receiver.Comp.ReceivingFrom.Value, EntityManager)),
                    ("item", Identity.Entity(realItem, EntityManager)), // FLoof - resolve virtual items
                    ("target", Identity.Entity(receiver, EntityManager))),
                sender,
                Filter.PvsExcept(sender, entityManager: EntityManager),
                true);
        }

        offererComponent.Item = null;
        UnReceive(receiver, receiver.Comp, offererComponent);
    }
    #endregion
    /// <summary>
    /// Returns true if <see cref="OfferItemComponent.IsInOfferMode"/> = true
    /// </summary>
    protected bool IsInOfferMode(Entity<OfferItemComponent?> ent)
    {
        return Resolve(ent, ref ent.Comp, false) && ent.Comp.IsInOfferMode;
    }

    private bool TryHandleExtendedTransfer(EntityUid user, EntityUid target, EntityUid offeredItem, EntityUid realItem)
    {
        var ev = new ItemTransferredEvent
        {
            User = user,
            Target = target,
            PassedItem = offeredItem,
            RealItem = realItem,
        };
        RaiseLocalEvent(realItem, ref ev);
        return ev.Handled;
    }
}

/// <summary>
///     Raised on the entity that was transferred via item offering.
/// </summary>
[ByRefEvent]
public sealed class ItemTransferredEvent : HandledEntityEventArgs
{
    public EntityUid User;
    public EntityUid Target;

    /// <summary>
    ///     The actual item being passed around. Can be a virtual item.
    /// </summary>
    public EntityUid PassedItem;
    /// <summary>
    ///     If <see cref="PassedItem"/> is a virtual item, this field contains the real item that was transferred.
    /// </summary>
    public EntityUid? RealItem;
}