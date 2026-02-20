using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
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

namespace Content.Shared._Floof.OfferItem;

public abstract partial class SharedOfferItemSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<OfferItemComponent, InteractUsingEvent>(OnInteractWithReceiver, before: [typeof(IngestionSystem)]);
        SubscribeLocalEvent<OfferableVirtualItemComponent, BeforeRangedInteractEvent>(OnRangedInteractWithReceiver);
        SubscribeLocalEvent<OfferItemComponent, MoveEvent>(OnMove);

        InitializeInteractions();
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

    /// <summary>
    ///     Attempts to create an offer. Expects offerer.Item to already be set to the offered item, offererComponent.InReceiveMode == true.
    ///     Will fail if offerer == receiver or if receiver already has a set TargetOrOfferer, and that person is not the current offerer
    /// </summary>
    private bool CreateOffer(Entity<OfferItemComponent> receiver, Entity<OfferItemComponent> offerer)
    {
        var offererComponent = offerer.Comp;
        var receiverComponent = receiver.Comp;
        if (offerer == receiver || receiverComponent.IsInReceiveMode || !offererComponent.IsInOfferMode ||
            (offererComponent.IsInReceiveMode && offererComponent.ReceivingFrom != receiver))
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
            receiver);

        return true;
    }

    private void OnMove(EntityUid uid, OfferItemComponent component, MoveEvent args)
    {
        if (_net.IsClient) // Client often mispredicts movement, we cant trust it here
            return;

        if (component.ReceivingFrom == null ||
            args.NewPosition.InRange(EntityManager, _transform,
                Transform(component.ReceivingFrom.Value).Coordinates, component.MaxOfferDistance))
            return;

        UnOffer(uid, component);
    }

    /// <summary>
    /// Resets the <see cref="_Floof.OfferItem.OfferItemComponent"/> of the user and the target
    /// </summary>
    protected void UnOffer(EntityUid thisEntity, OfferItemComponent offererComp)
    {
        if (!TryComp<HandsComponent>(thisEntity, out var hands) || _hands.GetActiveHand((thisEntity, hands)) is null)
            return;

        if (offererComp.ReceivingFrom is {} otherEntity && TryComp<OfferItemComponent>(otherEntity, out var otherOfferer))
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
    /// Returns true if <see cref="_Floof.OfferItem.OfferItemComponent.IsInOfferMode"/> = true
    /// </summary>
    protected bool IsInOfferMode(EntityUid? entity, OfferItemComponent? component = null)
    {
        return entity != null && Resolve(entity.Value, ref component, false) && component.IsInOfferMode;
    }
}
