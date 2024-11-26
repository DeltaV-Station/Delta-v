using Content.Server.Administration.Logs;
using Content.Server.CartridgeLoader;
using Content.Server.Power.Components;
using Content.Server.Radio;
using Content.Server.Radio.Components;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.Database;
using Content.Shared.DeltaV.CartridgeLoader.Cartridges;
using Content.Shared.DeltaV.NanoChat;
using Content.Shared.PDA;
using Content.Shared.Radio.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.DeltaV.CartridgeLoader.Cartridges;

public sealed class NanoChatCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridge = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly StationSystem _station = default!;

    // Messages in notifications get cut off after this point
    // no point in storing it on the comp
    private const int NotificationMaxLength = 64;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NanoChatCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<NanoChatCartridgeComponent, CartridgeMessageEvent>(OnMessage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Update card references for any cartridges that need it
        var query = EntityQueryEnumerator<NanoChatCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var nanoChat, out var cartridge))
        {
            if (cartridge.LoaderUid == null)
                continue;

            // Check if we need to update our card reference
            if (!TryComp<PdaComponent>(cartridge.LoaderUid, out var pda))
                continue;

            var newCard = pda.ContainedId;
            var currentCard = nanoChat.Card;

            // If the cards match, nothing to do
            if (newCard == currentCard)
                continue;

            // Update card reference
            nanoChat.Card = newCard;

            // Update UI state since card reference changed
            UpdateUI((uid, nanoChat), cartridge.LoaderUid.Value);
        }
    }

    /// <summary>
    ///     Handles incoming UI messages from the NanoChat cartridge.
    /// </summary>
    private void OnMessage(Entity<NanoChatCartridgeComponent> ent, ref CartridgeMessageEvent args)
    {
        if (args is not NanoChatUiMessageEvent msg)
            return;

        if (!GetCardEntity(GetEntity(args.LoaderUid), out var card))
            return;

        switch (msg.Type)
        {
            case NanoChatUiMessageType.NewChat:
                HandleNewChat(card, msg);
                break;
            case NanoChatUiMessageType.SelectChat:
                HandleSelectChat(card, msg);
                break;
            case NanoChatUiMessageType.CloseChat:
                HandleCloseChat(card);
                break;
            case NanoChatUiMessageType.ToggleMute:
                HandleToggleMute(card);
                break;
            case NanoChatUiMessageType.DeleteChat:
                HandleDeleteChat(card, msg);
                break;
            case NanoChatUiMessageType.SendMessage:
                HandleSendMessage(ent, card, msg);
                break;
        }

        UpdateUI(ent, GetEntity(args.LoaderUid));
    }

    /// <summary>
    ///     Gets the ID card entity associated with a PDA.
    /// </summary>
    /// <param name="loaderUid">The PDA entity ID</param>
    /// <param name="card">Output parameter containing the found card entity and component</param>
    /// <returns>True if a valid NanoChat card was found</returns>
    private bool GetCardEntity(
        EntityUid loaderUid,
        out Entity<NanoChatCardComponent> card)
    {
        card = default;

        // Get the PDA and check if it has an ID card
        if (!TryComp<PdaComponent>(loaderUid, out var pda) ||
            pda.ContainedId == null ||
            !TryComp<NanoChatCardComponent>(pda.ContainedId, out var idCard))
            return false;

        card = (pda.ContainedId.Value, idCard);
        return true;
    }

    /// <summary>
    ///     Handles creation of a new chat conversation.
    /// </summary>
    private void HandleNewChat(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (msg.RecipientNumber == null || msg.Content == null || msg.RecipientNumber == card.Comp.Number)
            return;

        // Add new recipient
        var recipient = new NanoChatRecipient(msg.RecipientNumber.Value,
            msg.Content,
            msg.RecipientJob);

        // Initialize or update recipient
        card.Comp.Recipients[msg.RecipientNumber.Value] = recipient;

        // Initialize empty message list if needed
        if (!card.Comp.Messages.ContainsKey(msg.RecipientNumber.Value))
            card.Comp.Messages[msg.RecipientNumber.Value] = new List<NanoChatMessage>();

        _adminLogger.Add(LogType.Action,
            LogImpact.Low,
            $"{ToPrettyString(msg.Actor):user} created new NanoChat conversation with #{msg.RecipientNumber:D4} ({msg.Content})");

        Dirty(card);
        UpdateUIForCard(card);
    }

    /// <summary>
    ///     Handles selecting a chat conversation.
    /// </summary>
    private void HandleSelectChat(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (msg.RecipientNumber == null)
            return;

        card.Comp.CurrentChat = msg.RecipientNumber;

        // Clear unread flag when selecting chat
        if (card.Comp.Recipients.TryGetValue(msg.RecipientNumber.Value, out var r))
        {
            r.HasUnread = false;
            card.Comp.Recipients[msg.RecipientNumber.Value] = r;
        }

        Dirty(card);
    }

    /// <summary>
    ///     Handles closing the current chat conversation.
    /// </summary>
    private void HandleCloseChat(Entity<NanoChatCardComponent> card)
    {
        card.Comp.CurrentChat = null;
        Dirty(card);
    }

    /// <summary>
    ///     Handles deletion of a chat conversation.
    /// </summary>
    private void HandleDeleteChat(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (msg.RecipientNumber == null || card.Comp.Number == null)
            return;

        // Remove the recipient, but keep the messages
        card.Comp.Recipients.Remove(msg.RecipientNumber.Value);

        // Clear current chat if we just deleted it
        if (card.Comp.CurrentChat == msg.RecipientNumber)
            card.Comp.CurrentChat = null;

        Dirty(card);
        UpdateUIForCard(card);
    }

    /// <summary>
    ///     Handles toggling notification mute state.
    /// </summary>
    private void HandleToggleMute(Entity<NanoChatCardComponent> card)
    {
        card.Comp.NotificationsMuted = !card.Comp.NotificationsMuted;
        Dirty(card);
        UpdateUIForCard(card);
    }

    /// <summary>
    ///     Handles sending a new message in a chat conversation.
    /// </summary>
    private void HandleSendMessage(Entity<NanoChatCartridgeComponent> cartridge,
        Entity<NanoChatCardComponent> card,
        NanoChatUiMessageEvent msg)
    {
        if (msg.RecipientNumber == null || msg.Content == null || card.Comp.Number == null)
            return;

        if (!EnsureRecipientExists(card, msg.RecipientNumber.Value))
            return;

        var (deliveryFailed, recipient) = AttemptMessageDelivery(cartridge, msg.RecipientNumber.Value);

        // Create and store message for sender
        var message = new NanoChatMessage(
            _timing.CurTime,
            msg.Content,
            (uint)card.Comp.Number,
            deliveryFailed
        );

        // Log message attempt
        var logRecipientText = recipient != null
            ? ToPrettyString(recipient.Value)
            : $"#{msg.RecipientNumber:D4}";

        _adminLogger.Add(LogType.Chat,
            LogImpact.Low,
            $"{ToPrettyString(card):user} sent NanoChat message to {logRecipientText}: {msg.Content}{(deliveryFailed ? " [DELIVERY FAILED]" : "")}");

        StoreMessage(card, msg.RecipientNumber.Value, message);

        if (!deliveryFailed && recipient != null)
            DeliverMessageToRecipient(card, recipient.Value, message);
    }

    /// <summary>
    ///     Ensures a recipient exists in the sender's contacts.
    /// </summary>
    /// <param name="card">The card to check contacts for</param>
    /// <param name="recipientNumber">The recipient's number to check</param>
    /// <returns>True if the recipient exists or was created successfully</returns>
    private bool EnsureRecipientExists(NanoChatCardComponent card, uint recipientNumber)
    {
        if (!card.Recipients.ContainsKey(recipientNumber))
        {
            var recipientInfo = GetCardInfo(recipientNumber);
            if (recipientInfo == null)
                return false;

            card.Recipients[recipientNumber] = recipientInfo.Value;
            if (!card.Messages.ContainsKey(recipientNumber))
                card.Messages[recipientNumber] = new List<NanoChatMessage>();
        }

        return true;
    }

    /// <summary>
    ///     Attempts to deliver a message to a recipient.
    /// </summary>
    /// <param name="sender">The sending cartridge entity</param>
    /// <param name="recipientNumber">The recipient's number</param>
    /// <returns>Tuple containing delivery status and recipient information if found.</returns>
    private (bool failed, Entity<NanoChatCardComponent>? recipient) AttemptMessageDelivery(
        Entity<NanoChatCartridgeComponent> sender,
        uint recipientNumber)
    {
        // First verify we can send from this device
        // We need to check the radio channel and telecomm status
        var channel = _prototype.Index(sender.Comp.RadioChannel);
        var sendAttemptEvent = new RadioSendAttemptEvent(channel, sender);
        RaiseLocalEvent(ref sendAttemptEvent);
        if (sendAttemptEvent.Cancelled)
            return (true, null);

        Entity<NanoChatCardComponent>? foundRecipient = null;

        // First find the recipient's card
        var cardQuery = EntityQueryEnumerator<NanoChatCardComponent>();

        while (cardQuery.MoveNext(out var cardUid, out var card))
        {
            if (card.Number != recipientNumber)
                continue;

            foundRecipient = (cardUid, card);

            // Now find any cartridges that have this card
            var cartridgeQuery = EntityQueryEnumerator<NanoChatCartridgeComponent, ActiveRadioComponent>();
            var foundValidCartridge = false;

            while (cartridgeQuery.MoveNext(out var receiverUid, out var receiverCart, out _))
            {
                if (receiverCart.Card != cardUid)
                    continue;

                // Check if devices are on same station/map
                var recipientStation = _station.GetOwningStation(receiverUid);
                var senderStation = _station.GetOwningStation(sender);

                // Both entities must be on a station
                if (recipientStation == null || senderStation == null)
                    continue;

                // Must be on same map/station unless long range allowed
                if (!channel.LongRange && recipientStation != senderStation)
                    continue;

                // Needs telecomms
                if (!HasActiveServer(senderStation.Value) || !HasActiveServer(recipientStation.Value))
                    continue;

                // Check if recipient can receive
                var receiveAttemptEv = new RadioReceiveAttemptEvent(channel, sender, receiverUid);
                RaiseLocalEvent(ref receiveAttemptEv);
                if (receiveAttemptEv.Cancelled)
                    continue;

                // Found a valid cartridge that can receive
                foundValidCartridge = true;
                break;
            }

            if (foundValidCartridge)
                return (false, foundRecipient);

            break; // Found card but no valid cartridge
        }

        return (true, foundRecipient);
    }

    /// <summary>
    ///     Checks if there are any active telecomms servers on the given station
    /// </summary>
    private bool HasActiveServer(EntityUid station)
    {
        // I have no idea why this isn't public in the RadioSystem
        var query =
            EntityQueryEnumerator<TelecomServerComponent, EncryptionKeyHolderComponent, ApcPowerReceiverComponent>();

        while (query.MoveNext(out var uid, out _, out _, out var power))
        {
            if (_station.GetOwningStation(uid) == station && power.Powered)
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Stores a message in the sender's message history.
    /// </summary>
    /// <param name="card">The sender's card</param>
    /// <param name="recipientNumber">The recipient's number</param>
    /// <param name="message">The message to store</param>
    private void StoreMessage(Entity<NanoChatCardComponent> card, uint recipientNumber, NanoChatMessage message)
    {
        // Make sure we have a message list for this conversation
        if (!card.Comp.Messages.ContainsKey(recipientNumber))
            card.Comp.Messages[recipientNumber] = new List<NanoChatMessage>();

        card.Comp.Messages[recipientNumber].Add(message);
        card.Comp.LastMessageTime = _timing.CurTime;
        Dirty(card);
    }

    /// <summary>
    ///     Delivers a message to the recipient and handles associated notifications.
    /// </summary>
    /// <param name="sender">The sender's card entity</param>
    /// <param name="recipient">The recipient's card entity</param>
    /// <param name="message">The <see cref="NanoChatMessage" /> to deliver</param>
    private void DeliverMessageToRecipient(Entity<NanoChatCardComponent> sender,
        Entity<NanoChatCardComponent> recipient,
        NanoChatMessage message)
    {
        // Add sender as contact if needed
        if (!recipient.Comp.Recipients.ContainsKey((uint)sender.Comp.Number!))
        {
            var senderInfo = GetCardInfo((uint)sender.Comp.Number!);
            if (senderInfo != null)
            {
                recipient.Comp.Recipients[(uint)sender.Comp.Number!] = senderInfo.Value;
                if (!recipient.Comp.Messages.ContainsKey((uint)sender.Comp.Number!))
                    recipient.Comp.Messages[(uint)sender.Comp.Number!] = new List<NanoChatMessage>();
            }
        }

        // Store message in recipient's inbox
        recipient.Comp.Messages[(uint)sender.Comp.Number!].Add(message with { DeliveryFailed = false });

        // Mark as unread if recipient isn't viewing this chat
        if (recipient.Comp.CurrentChat != sender.Comp.Number)
            HandleUnreadNotification(recipient, message);

        Dirty(recipient);
        UpdateUIForCard(recipient);
    }

    /// <summary>
    ///     Handles unread message notifications and updates unread status.
    /// </summary>
    private void HandleUnreadNotification(Entity<NanoChatCardComponent> recipient, NanoChatMessage message)
    {
        // Get sender name from contacts or fall back to number
        var senderName = recipient.Comp.Recipients.TryGetValue(message.SenderId, out var existingRecipient)
            ? existingRecipient.Name
            : $"#{message.SenderId:D4}";

        if (!recipient.Comp.Recipients[message.SenderId].HasUnread && !recipient.Comp.NotificationsMuted)
        {
            var pdaQuery = EntityQueryEnumerator<PdaComponent>();
            while (pdaQuery.MoveNext(out var pdaUid, out var pdaComp))
            {
                if (pdaComp.ContainedId != recipient)
                    continue;

                _cartridge.SendNotification(pdaUid,
                    Loc.GetString("nano-chat-new-message-title", ("sender", senderName)),
                    Loc.GetString("nano-chat-new-message-body", ("message", TruncateMessage(message.Content))));
                break;
            }
        }

        // Update unread status
        recipient.Comp.Recipients[message.SenderId] =
            recipient.Comp.Recipients[message.SenderId] with { HasUnread = true };
    }

    /// <summary>
    ///     Updates the UI for any PDAs containing the specified card.
    /// </summary>
    private void UpdateUIForCard(EntityUid cardUid)
    {
        // Find any PDA containing this card and update its UI
        var query = EntityQueryEnumerator<NanoChatCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp, out var cartridge))
        {
            if (comp.Card != cardUid || cartridge.LoaderUid == null)
                continue;

            UpdateUI((uid, comp), cartridge.LoaderUid.Value);
        }
    }

    /// <summary>
    ///     Gets the <see cref="NanoChatRecipient" /> for a given NanoChat number.
    /// </summary>
    private NanoChatRecipient? GetCardInfo(uint number)
    {
        // Find card with this number to get its info
        var query = EntityQueryEnumerator<NanoChatCardComponent>();
        while (query.MoveNext(out var uid, out var card))
        {
            if (card.Number != number)
                continue;

            // Try to get job title from ID card if possible
            string? jobTitle = null;
            var name = "Unknown";
            if (TryComp<IdCardComponent>(uid, out var idCard))
            {
                jobTitle = idCard.LocalizedJobTitle;
                name = idCard.FullName ?? name;
            }

            return new NanoChatRecipient(number, name, jobTitle);
        }

        return null;
    }

    /// <summary>
    ///     Truncates a message to the notification maximum length.
    /// </summary>
    private static string TruncateMessage(string message)
    {
        return message.Length <= NotificationMaxLength
            ? message
            : message[..(NotificationMaxLength - 4)] + " [...]";
    }

    private void OnUiReady(Entity<NanoChatCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        _cartridge.RegisterBackgroundProgram(args.Loader, ent);
        UpdateUI(ent, args.Loader);
    }

    private void UpdateUI(Entity<NanoChatCartridgeComponent> ent, EntityUid loader)
    {
        if (_station.GetOwningStation(loader) is { } station)
            ent.Comp.Station = station;

        var recipients = new Dictionary<uint, NanoChatRecipient>();
        var messages = new Dictionary<uint, List<NanoChatMessage>>();
        uint? currentChat = null;
        uint ownNumber = 0;
        var maxRecipients = 50;
        var notificationsMuted = false;

        if (ent.Comp.Card != null && TryComp<NanoChatCardComponent>(ent.Comp.Card, out var card))
        {
            recipients = card.Recipients;
            messages = card.Messages;
            currentChat = card.CurrentChat;
            ownNumber = card.Number ?? 0;
            maxRecipients = card.MaxRecipients;
            notificationsMuted = card.NotificationsMuted;
        }

        var state = new NanoChatUiState(recipients,
            messages,
            currentChat,
            ownNumber,
            maxRecipients,
            notificationsMuted);
        _cartridge.UpdateCartridgeUiState(loader, state);
    }
}
