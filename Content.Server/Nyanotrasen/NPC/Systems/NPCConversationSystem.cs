using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Server.Chat.Systems;
using Content.Server.Chat.TypingIndicator;
using Content.Server.NPC.Components;
using Content.Server.NPC.Events;
using Content.Server.NPC.Prototypes;
using Content.Server.Speech;
using Content.Shared.Interaction;

namespace Content.Server.NPC.Systems;

public sealed class NPCConversationSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly NPCSystem _npcSystem = default!;
    [Dependency] private readonly RotateToFaceSystem _rotateToFaceSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly TypingIndicatorSystem _typingIndicatorSystem = default!;

    private ISawmill _sawmill = default!;

    // TODO: attention attenuation. distance, facing, visible
    // TODO: attending to multiple people, multiple streams of conversation
    // TODO: multi-word prompts
    // TODO: nameless prompting (pointing is good)
    // TODO: aliases

    public static readonly string[] QuestionWords = { "who", "what", "when", "why", "where", "how" };
    public static readonly string[] Copulae = { "is", "are" };

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("npc.conversation");

        SubscribeLocalEvent<NPCConversationComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<NPCConversationComponent, EntityUnpausedEvent>(OnUnpaused);
        SubscribeLocalEvent<NPCConversationComponent, ListenAttemptEvent>(OnListenAttempt);
        SubscribeLocalEvent<NPCConversationComponent, ListenEvent>(OnListen);

        SubscribeLocalEvent<NPCConversationComponent, NPCConversationByeEvent>(OnBye);
        SubscribeLocalEvent<NPCConversationComponent, NPCConversationHelpEvent>(OnHelp);

        SubscribeLocalEvent<NPCConversationComponent, NPCConversationToldNameEvent>(OnToldName);
    }

#region API

    /// <summary>
    /// Toggle the ability of an NPC to listen for topics.
    /// </summary>
    public void EnableConversation(EntityUid uid, bool enable = true, NPCConversationComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.Enabled = enable;
    }

    /// <summary>
    /// Toggle the NPC's willingness to make idle comments.
    /// </summary>
    public void EnableIdleChat(EntityUid uid, bool enable = true, NPCConversationComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.IdleEnabled = enable;
    }

    /// <summary>
    /// Return locked status of a dialogue topic.
    /// </summary>
    public bool IsDialogueLocked(EntityUid uid, string option, NPCConversationComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return true;

        if (!component.ConversationTree.PromptToTopic.TryGetValue(option, out var topic))
        {
            _sawmill.Warning($"Tried to check locked status of missing dialogue option `{option}` on {ToPrettyString(uid)}");
            return true;
        }

        if (component.UnlockedTopics.Contains(topic))
            return false;

        return topic.Locked;
    }

    /// <summary>
    /// Unlock dialogue options normally locked in an NPC's conversation tree.
    /// </summary>
    public void UnlockDialogue(EntityUid uid, string option, NPCConversationComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.ConversationTree.PromptToTopic.TryGetValue(option, out var topic))
            component.UnlockedTopics.Add(topic);
        else
            _sawmill.Warning($"Tried to unlock missing dialogue option `{option}` on {ToPrettyString(uid)}");
    }

    /// <inheritdoc cref="UnlockDialogue(EntityUid, string, NPCConversationComponent?)"/>
    public void UnlockDialogue(EntityUid uid, HashSet<string> options, NPCConversationComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        foreach (var option in options)
            UnlockDialogue(uid, option, component);
    }

    /// <summary>
    /// Queue a response for an NPC with a visible typing indicator and delay between messages.
    /// </summary>
    /// <remarks>
    /// This can be used as opposed to the typical <see cref="ChatSystem.TrySendInGameICMessage"/> method.
    /// </remarks>
    public void QueueResponse(EntityUid uid, NPCResponse response, NPCConversationComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (response.Is is {} ev)
        {
            // This is a dynamic response which will call QueueResponse with static responses of its own.
            ev.TalkingTo = component.AttendingTo;
            RaiseLocalEvent(uid, (object) ev);
            return;
        }

        if (component.ResponseQueue.Count == 0)
        {
            DelayResponse(uid, component, response);
            _typingIndicatorSystem.SetTypingIndicatorEnabled(uid, true);
        }

        component.ResponseQueue.Push(response);
    }

    /// <summary>
    /// Make an NPC stop paying attention to someone.
    /// </summary>
    public void LoseAttention(EntityUid uid, NPCConversationComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.AttendingTo = null;
        component.ListeningEvent = null;
        _rotateToFaceSystem.TryFaceAngle(uid, component.OriginalFacing);
    }

#endregion

    private void DelayResponse(EntityUid uid, NPCConversationComponent component, NPCResponse response)
    {
        if (response.Text == null)
            return;

        component.NextResponse = _gameTiming.CurTime +
            component.DelayBeforeResponse +
            component.TypingDelay.TotalSeconds * TimeSpan.FromSeconds(response.Text.Length) *
            _random.NextDouble(0.9, 1.1);
    }

    private IEnumerable<NPCTopic> GetAvailableTopics(EntityUid uid, NPCConversationComponent component)
    {
        HashSet<NPCTopic> availableTopics = new();

        foreach (var topic in component.ConversationTree.Dialogue)
        {
            if (!topic.Locked || component.UnlockedTopics.Contains(topic))
                availableTopics.Add(topic);
        }

        return availableTopics;
    }

    private IEnumerable<NPCTopic> GetVisibleTopics(EntityUid uid, NPCConversationComponent component)
    {
        HashSet<NPCTopic> visibleTopics = new();

        foreach (var topic in component.ConversationTree.Dialogue)
        {
            if (!topic.Hidden && (!topic.Locked || component.UnlockedTopics.Contains(topic)))
                visibleTopics.Add(topic);
        }

        return visibleTopics;
    }

    private void OnInit(EntityUid uid, NPCConversationComponent component, ComponentInit args)
    {
        if (component.ConversationTreeId == null)
            return;

        component.ConversationTree = _prototype.Index<NPCConversationTreePrototype>(component.ConversationTreeId);
        component.NextIdleChat = _gameTiming.CurTime + component.IdleChatDelay;

        for (var i = 0; i < component.ConversationTree.Idle.Length; ++i)
            component.IdleChatOrder.Add(i);

        _random.Shuffle(component.IdleChatOrder);
    }

    private void OnUnpaused(EntityUid uid, NPCConversationComponent component, ref EntityUnpausedEvent args)
    {
        component.NextResponse += args.PausedTime;
        component.NextAttentionLoss += args.PausedTime;
        component.NextIdleChat += args.PausedTime;
    }

    private bool TryGetIdleChatLine(EntityUid uid, NPCConversationComponent component, [NotNullWhen(true)] out NPCResponse? line)
    {
        line = null;

        if (component.IdleChatOrder.Count() == 0)
            return false;

        if (++component.IdleChatIndex == component.IdleChatOrder.Count())
        {
            // Exhausted all lines in the pre-shuffled order.
            // Reset the index and shuffle again.
            component.IdleChatIndex = 0;
            _random.Shuffle(component.IdleChatOrder);
        }

        var index = component.IdleChatOrder[component.IdleChatIndex];

        line = component.ConversationTree.Idle[index];

        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NPCConversationComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            var curTime = _gameTiming.CurTime;

            if (curTime >= component.NextResponse && component.ResponseQueue.Count > 0)
            {
                // Make a response.
                Respond(uid, component, component.ResponseQueue.Pop());
            }

            if (curTime >= component.NextAttentionLoss && component.AttendingTo != null)
            {
                // Forget who we were talking to.
                LoseAttention(uid, component);
            }

            if (component.IdleEnabled &&
                curTime >= component.NextIdleChat &&
                TryGetIdleChatLine(uid, component, out var line))
            {
                Respond(uid, component, line);
            }
        }
    }

    private void OnListenAttempt(EntityUid uid, NPCConversationComponent component, ListenAttemptEvent args)
    {
        if (!component.Enabled ||
            // Don't listen to myself...
            uid == args.Source ||
            // Don't bother listening to other NPCs. For now.
            !HasComp<ActorComponent>(args.Source) ||
            // We're already "typing" a response, so do that first.
            component.ResponseQueue.Count > 0)
        {
            args.Cancel();
        }
    }

    private void PayAttentionTo(EntityUid uid, NPCConversationComponent component, EntityUid speaker)
    {
        component.AttendingTo = speaker;
        component.NextAttentionLoss = _gameTiming.CurTime + component.AttentionSpan;
        component.OriginalFacing = _transformSystem.GetWorldRotation(uid);
    }

    private void Respond(EntityUid uid, NPCConversationComponent component, NPCResponse response)
    {
        if (component.ResponseQueue.Count == 0)
            _typingIndicatorSystem.SetTypingIndicatorEnabled(uid, false);
        else
            DelayResponse(uid, component, component.ResponseQueue.Peek());

        if (component.AttendingTo != null)
        {
            // TODO: This line is a mouthful. Maybe write a public API that supports EntityCoordinates later?
            var speakerCoords = Transform(component.AttendingTo.Value).Coordinates.ToMap(EntityManager, _transformSystem).Position;
            _rotateToFaceSystem.TryFaceCoordinates(uid, speakerCoords);
        }

        if (response.Event is {} ev)
        {
            ev.TalkingTo = component.AttendingTo;
            RaiseLocalEvent(uid, (object) ev);
        }

        if (response.ListenEvent != null)
            component.ListeningEvent = response.ListenEvent;

        if (response.Text != null)
            _chatSystem.TrySendInGameICMessage(uid, Loc.GetString(response.Text), InGameICChatType.Speak, false);

        if (response.Audio != null)
            _audioSystem.PlayPvs(response.Audio, uid,
                // TODO: Allow this to be configured per NPC/response.
                AudioParams.Default
                    .WithVolume(8f)
                    .WithMaxDistance(9f)
                    .WithRolloffFactor(0.5f));

        // Refresh our attention.
        component.NextAttentionLoss = _gameTiming.CurTime + component.AttentionSpan;
        component.NextIdleChat = component.NextAttentionLoss + component.IdleChatDelay;
    }

    private List<string> ParseMessageIntoWords(string message)
    {
        return Regex.Replace(message.Trim().ToLower(), @"(\p{P})", "")
            .Split()
            .ToList();
    }

    private bool FindResponse(EntityUid uid, NPCConversationComponent component, List<string> words, [NotNullWhen(true)] out NPCResponse? response)
    {
        response = null;

        var availableTopics = GetAvailableTopics(uid, component);

        // Some topics are more interesting than others.
        var greatestWeight = 0f;
        NPCTopic? candidate = null;

        foreach (var word in words)
        {
            if (component.ConversationTree.PromptToTopic.TryGetValue(word, out var topic) &&
                availableTopics.Contains(topic) &&
                topic.Weight > greatestWeight)
            {
                greatestWeight = topic.Weight;
                candidate = topic;
            }
        }

        if (candidate != null)
        {
            response = _random.Pick(candidate.Responses);
            return true;
        }

        return false;
    }

    private bool JudgeQuestionLikelihood(EntityUid uid, NPCConversationComponent component, List<string> words, string message)
    {
        if (message.Length > 0 && message[^1] == '?')
            // A question mark is an absolute mark of a question.
            return true;

        if (words.Count == 1)
            // The usefulness of this is dubious, but it's definitely a question.
            return QuestionWords.Contains(words[0]);

        if (words.Count >= 2)
            return QuestionWords.Contains(words[0]) && Copulae.Contains(words[1]);

        return false;
    }

    private void OnBye(EntityUid uid, NPCConversationComponent component, NPCConversationByeEvent args)
    {
        LoseAttention(uid, component);
    }

    private void OnHelp(EntityUid uid, NPCConversationComponent component, NPCConversationHelpEvent args)
    {
        if (args.Text == null)
        {
            _sawmill.Error($"{ToPrettyString(uid)} heard a Help prompt but has no text for it.");
            return;
        }

        var availableTopics = GetVisibleTopics(uid, component);
        var availablePrompts = availableTopics.Select(topic => topic.Prompts.FirstOrDefault()).ToArray();

        string availablePromptsText;
        if (availablePrompts.Count() <= 2)
        {
            availablePromptsText = Loc.GetString(args.Text,
                ("availablePrompts", string.Join(" or ", availablePrompts))
            );
        }
        else
        {
            availablePrompts[^1] = $"or {availablePrompts[^1]}";
            availablePromptsText = Loc.GetString(args.Text,
                ("availablePrompts", string.Join(", ", availablePrompts))
            );
        }

        // Unlikely we'll be able to do audio that isn't hard-coded,
        // so best to keep it general.
        var response = new NPCResponse(availablePromptsText, args.Audio);
        QueueResponse(uid, response, component);
    }

    private void OnToldName(EntityUid uid, NPCConversationComponent component, NPCConversationListenEvent args)
    {
        if (!component.ConversationTree.Custom.TryGetValue("toldName", out var responses))
            return;

        var response = _random.Pick(responses);
        if (response.Text == null)
        {
            _sawmill.Error($"{ToPrettyString(uid)} was told a name but had no text response.");
            return;
        }

        // The world's simplest heuristic for names:
        if (args.Words.Count > 3)
        {
            // It didn't seem like a name, so wait for something that does.
            return;
        }

        var cleanedName = string.Join(" ", args.Words);
        cleanedName = char.ToUpper(cleanedName[0]) + cleanedName.Remove(0, 1);

        var formattedResponse = new NPCResponse(Loc.GetString(response.Text,
                ("name", cleanedName)),
                response.Audio);

        QueueResponse(uid, formattedResponse, component);
        args.Handled = true;
    }

    private void OnListen(EntityUid uid, NPCConversationComponent component, ListenEvent args)
    {
        if (component.AttendingTo != null && component.AttendingTo != args.Source)
            // Ignore someone speaking to us if we're already paying attention to someone else.
            return;

        var words = ParseMessageIntoWords(args.Message);
        if (words.Count == 0)
            return;

        if (component.AttendingTo == args.Source)
        {
            // The person we're talking to said something to us.

            if (component.ListeningEvent is {} ev)
            {
                // We were waiting on this person to say something, and they've said something.
                ev.Handled = false;
                ev.Speaker = component.AttendingTo;
                ev.Message = args.Message;
                ev.Words = words;
                RaiseLocalEvent(uid, (object) ev);

                if (ev.Handled)
                    component.ListeningEvent = null;

                return;
            }

            // We're already paying attention to this person,
            // so try to figure out if they said something we can talk about.
            if (FindResponse(uid, component, words, out var response))
            {
                // A response was found so go ahead with it.
                QueueResponse(uid, response,  component);
            }
            else if(JudgeQuestionLikelihood(uid, component, words, args.Message))
            {
                // The message didn't match any of the prompts, but it seemed like a question.
                var unknownResponse = _random.Pick(component.ConversationTree.Unknown);
                QueueResponse(uid, unknownResponse,  component);
            }

            // If the message didn't seem like a question,
            // and it didn't raise any of our topics,
            // then politely ignore who we're talking with.
            //
            // It's better than spamming them with "I don't understand."
            return;
        }

        // See if someone said our name.
        var myName = MetaData(uid).EntityName.ToLower();

        // So this is a rough heuristic, but if our name occurs within the first three words,
        // or is the very last one, someone might be trying to talk to us.
        var payAttention = words[0] == myName || words[^1] == myName;
        if (!payAttention)
        {
            for (int i = 1; i < Math.Min(2, words.Count); ++i)
            {
                if (words[i] == myName)
                {
                    payAttention = true;
                    break;
                }
            }
        }

        if (payAttention)
        {
            PayAttentionTo(uid, component, args.Source);

            if (!FindResponse(uid, component, words, out var response))
            {
                if(JudgeQuestionLikelihood(uid, component, words, args.Message) &&
                    // This subcondition exists to block our name being interpreted as a question in its own right.
                    words.Count > 1)
                {
                    response = _random.Pick(component.ConversationTree.Unknown);
                }
                else
                {
                    response = _random.Pick(component.ConversationTree.Attention);
                }
            }

            QueueResponse(uid, response, component);
        }
    }
}

