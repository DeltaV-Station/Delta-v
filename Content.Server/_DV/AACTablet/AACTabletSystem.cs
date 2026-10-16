using Content.Shared.Chat;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.Speech.Components;
using Content.Shared._DV.AACTablet;
using Content.Shared._DV.AACTablet.Components;
using Content.Shared._DV.AACTablet.Events;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Radio.Components;
using Content.Shared.Radio;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._DV.AACTablet;

public sealed class AACTabletSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;

    private readonly List<string> _localisedPhrases = [];

    public const int MaxPhrases = 10; // no writing novels

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AACTabletComponent, AACTabletSendPhraseMessage>(OnSendPhrase);
        SubscribeLocalEvent<AACTabletComponent, ActivatableUIOpenAttemptEvent>(OnAACOpenAttempt);

        Subs.BuiEvents<AACTabletComponent>(AACTabletKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnBoundUIOpened);
        });
    }

    private HashSet<ProtoId<RadioChannelPrototype>> GetAvailableChannels(EntityUid entity)
    {
        var channels = new HashSet<ProtoId<RadioChannelPrototype>>();

        // Get all the intrinsic radio channels (IPCs, implants)
        if (TryComp(entity, out ActiveRadioComponent? intrinsicRadio))
            channels.UnionWith(intrinsicRadio.Channels);

        // Get the user's headset channels, if any
        if (TryComp(entity, out WearingHeadsetComponent? headset)
            && TryComp(headset.Headset, out ActiveRadioComponent? headsetRadio))
            channels.UnionWith(headsetRadio.Channels);

        return channels;
    }

    private void OnBoundUIOpened(Entity<AACTabletComponent> ent, ref BoundUIOpenedEvent args)
    {
        var state = new AACTabletBuiState(GetAvailableChannels(args.Actor));
        _userInterface.SetUiState(args.Entity, AACTabletKey.Key, state);
    }

    private void OnSendPhrase(Entity<AACTabletComponent> ent, ref AACTabletSendPhraseMessage message)
    {
        if (ent.Comp.NextPhrase > _timing.CurTime || message.PhraseIds.Count > MaxPhrases)
            return;

        var senderName = Identity.Entity(message.Actor, EntityManager);
        var speakerName = Loc.GetString("speech-name-relay",
            ("speaker", Name(ent)),
            ("originalName", senderName));

        _localisedPhrases.Clear();
        foreach (var phraseProto in message.PhraseIds)
        {
            if (_prototype.Resolve(phraseProto, out var phrase))
            {
                // Ensures each phrase is capitalised to maintain common AAC styling
                _localisedPhrases.Add(_chat.SanitizeMessageCapital(Loc.GetString(phrase.Text)));
            }
        }

        if (_localisedPhrases.Count <= 0)
            return;

        EnsureComp<VoiceOverrideComponent>(ent).NameOverride = speakerName;

        // Set the player's currently available channels before sending the message
        EnsureComp(ent, out IntrinsicRadioTransmitterComponent transmitter);
        transmitter.Channels = GetAvailableChannels(message.Actor);

        // L5 — save the message for logging
        var messageToSend = string.Join(" ", _localisedPhrases);

        _chat.TrySendInGameICMessage(ent,
            message.Prefix + messageToSend,
            InGameICChatType.Speak,
            hideChat: false,
            nameOverride: speakerName);

        // L5 — log AAC chat message
        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"AAC tablet message from {ToPrettyString(message.Actor):user}: {messageToSend}");

        var curTime = _timing.CurTime;
        ent.Comp.NextPhrase = curTime + ent.Comp.Cooldown;
    }

    private void OnAACOpenAttempt(Entity<AACTabletComponent> tablet, ref ActivatableUIOpenAttemptEvent args)
    {
        var ev = new AACTabletOpenAttemptEvent(tablet);
        RaiseLocalEvent(args.User, ref ev);

        if (!ev.Cancelled)
            return;

        args.Cancel();
        if (ev.FailReason == null)
            return;

        var message = Loc.GetString(ev.FailReason);
        _popup.PopupPredicted(message, args.User, args.User, PopupType.MediumCaution);
    }
}
