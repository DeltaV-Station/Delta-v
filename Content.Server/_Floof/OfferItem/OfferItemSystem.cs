using Content.Server.Popups;
using Content.Shared._Floof.OfferItem;
using Content.Shared.Alert;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Robust.Shared.Player;

namespace Content.Server._Floof.OfferItem;

public sealed partial class OfferItemSystem : SharedOfferItemSystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    // Floofstation
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OfferItemComponent, AcceptOfferAlertEvent>(OnAcceptOffer);
        InitializeTransfers();
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

    private void OnAcceptOffer(Entity<OfferItemComponent> ent, ref AcceptOfferAlertEvent args)
    {
        Receive(ent, ent);
    }

    /// <summary>
    /// Accepting the offer and receive item
    /// </summary>
    public void Receive(EntityUid receiver, OfferItemComponent? receiverComponent = null)
    {
        if (!Resolve(receiver, ref receiverComponent) ||
            !TryComp<OfferItemComponent>(receiverComponent.ReceivingFrom, out var offererComponent) ||
            offererComponent.Hand == null ||
            receiverComponent.ReceivingFrom is not {} sender ||
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
                    ("user", Identity.Entity(receiverComponent.ReceivingFrom.Value, EntityManager)),
                    ("item", Identity.Entity(realItem, EntityManager)), // FLoof - resolve virtual items
                    ("target", Identity.Entity(receiver, EntityManager))),
                sender,
                Filter.PvsExcept(sender, entityManager: EntityManager),
                true);
        }

        offererComponent.Item = null;
        UnReceive(receiver, receiverComponent, offererComponent);
    }
}
