using Content.Server.Chat.Managers;
using Content.Shared._DV.SignLanguage;
using Content.Shared._DV.SignLanguage.Prototypes;
using Content.Shared.Chat;
using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Traits.Assorted;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.SignLanguage;

/// <summary>
/// Server-side system that handles sign language communication.
/// Receives sign language messages and broadcasts them to entities in range
/// with appropriate formatting based on whether they understand sign language.
/// </summary>
public sealed class SignLanguageSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    /// <summary>
    /// Range for sign language visibility. Players must be within this range
    /// and have line of sight to see the sign.
    /// </summary>
    private const float SignLanguageRange = 7f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<PerformSignLanguageMessage>(OnPerformSignLanguage);
    }

    private void OnPerformSignLanguage(PerformSignLanguageMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } signer)
            return;

        if (!HasComp<KnowsSignLanguageComponent>(signer))
            return;

        // Verify the signer has at least one free hand
        if (_hands.CountFreeHands(signer) < 1)
        {
            _popup.PopupEntity(Loc.GetString("sign-language-need-free-hand"), signer, signer);
            return;
        }

        // Validate prototypes exist
        if (!_prototype.TryIndex(msg.TopicId, out var topic) ||
            !_prototype.TryIndex(msg.EventId, out var eventProto) ||
            !_prototype.TryIndex(msg.IntentId, out var intent))
            return;

        SignIntensityPrototype? intensity = null;
        if (msg.IntensityId != null)
            _prototype.TryIndex(msg.IntensityId.Value, out intensity);

        // Get signer's map coordinates for range/occlusion checks
        var signerPos = _transform.GetMapCoordinates(signer);

        // Get localized names for the sign components
        var topicName = Loc.GetString(topic.Name);
        var eventName = Loc.GetString(eventProto.Name);
        var intentName = Loc.GetString(intent.Name);
        var intensityId = intensity?.ID ?? string.Empty;
        var formatting = intensity?.TextFormatting ?? string.Empty;
        var boldMessage = intensity?.BoldMessage ?? false;

        // Get non-fluent description
        var nonFluentDescription = intensity != null
            ? Loc.GetString(intensity.NonFluentDescription)
            : Loc.GetString("sign-language-nonfluent-default-description");

        // Find all players in range with line of sight and send appropriate messages
        var query = EntityQueryEnumerator<ActorComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var actor, out var xform))
        {
            var observerPos = _transform.GetMapCoordinates(uid, xform);

            // Check range and line of sight using examine system
            if (!_examine.InRangeUnOccluded(signerPos, observerPos, SignLanguageRange, null, true, _ent))
                continue;

            // You're blind, duh
            if (HasComp<PermanentBlindnessComponent>(uid) || HasComp<TemporaryBlindnessComponent>(uid))
                return;

            // Determine if observer understands sign language
            var understands = HasComp<KnowsSignLanguageComponent>(uid);

            string message;
            string wrappedMessage;

            if (understands)
            {
                message = Loc.GetString("sign-language-fluent-message",
                    ("intensity", intensityId),
                    ("topic", topicName),
                    ("event", eventName),
                    ("intent", intentName),
                    ("formatting", formatting));

                // Pick the appropriate wrapper based on bold setting
                var wrapperKey = boldMessage
                    ? "sign-language-wrap-fluent-bold"
                    : "sign-language-wrap-fluent";

                wrappedMessage = Loc.GetString(wrapperKey,
                    ("entity", signer),
                    ("entityName", Name(signer)),
                    ("intensity", intensityId),
                    ("topic", topicName),
                    ("event", eventName),
                    ("intent", intentName),
                    ("formatting", formatting));
            }
            else
            {
                message = Loc.GetString("sign-language-nonfluent-message",
                    ("description", nonFluentDescription),
                    ("topic", topicName));

                wrappedMessage = Loc.GetString("sign-language-wrap-nonfluent",
                    ("entity", signer),
                    ("entityName", Name(signer)),
                    ("description", nonFluentDescription),
                    ("topic", topicName));
            }

            _chat.ChatMessageToOne(
                ChatChannel.Emotes,
                message,
                wrappedMessage,
                signer,
                hideChat: false,
                actor.PlayerSession.Channel);
        }
    }
}
