using Content.Shared.ActionBlocker;
using Content.Shared.Hands.Components;
using Content.Shared.Input;
using Content.Shared.Popups;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Shared._Floof.OfferItem;

public abstract partial class SharedOfferItemSystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private void InitializeInteractions()
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OfferItem, InputCmdHandler.FromDelegate(SetInOfferMode, handle: false, outsidePrediction: false))
            .Register<SharedOfferItemSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<SharedOfferItemSystem>();
    }

    /// <summary>
    ///     This sets IsInOfferMode to true, allowing the player to select whom to offer an item to with interaction.
    /// </summary>
    private void SetInOfferMode(ICommonSession? offerer)
    {
        if (offerer is not { } playerSession)
            return;

        if ((playerSession.AttachedEntity is not { Valid: true } uid || !Exists(uid)) ||
            !_actionBlocker.CanInteract(uid, null))
            return;

        if (!TryComp<OfferItemComponent>(uid, out var offerItem))
            return;

        if (!TryComp<HandsComponent>(uid, out var hands)
            || _hands.GetActiveHand((uid, hands)) is not {} activeHandName
            || !_hands.TryGetHeldItem((uid, hands), activeHandName, out var heldItem))
            return;

        offerItem.Item = heldItem;

        if (!offerItem.IsInOfferMode)
        {
            if (offerItem.Item == null)
            {
                _popup.PopupEntity(Loc.GetString("offer-item-empty-hand"), uid, uid);
                return;
            }

            if (offerItem.Hand == null || offerItem.ReceivingFrom == null)
            {
                offerItem.IsInOfferMode = true;
                offerItem.Hand = activeHandName;

                Dirty(uid, offerItem);
                return;
            }
        }

        // If we're already offering an item to someone, cancel that offer
        if (offerItem.ReceivingFrom != null)
        {
            UnReceive(offerItem.ReceivingFrom.Value, offererComp: offerItem);
            offerItem.IsInOfferMode = false;
            Dirty(uid, offerItem);
            return;
        }

        UnOffer(uid, offerItem);
    }
}
