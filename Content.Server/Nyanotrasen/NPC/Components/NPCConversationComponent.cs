using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Server.NPC.Events;
using Content.Server.NPC.Prototypes;
using Content.Server.NPC.Systems;

namespace Content.Server.NPC.Components;

[RegisterComponent]
[Access(typeof(NPCConversationSystem))]
public sealed class NPCConversationComponent : Component
{
    /// <summary>
    /// Whether or not the listening logic is turned on.
    /// </summary>
    /// <remarks>
    /// Queued responses will still play through, but no new attempts to listen will be made.
    /// </remarks>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("enabled")]
    public bool Enabled = true;

    /* NYI:
    /// <summary>
    /// The NPC will pay attention when one of these words are said.
    /// </summary>
    [ViewVariables]
    [DataField("aliases")]
    public List<string> Aliases = new();
    */

    [ViewVariables]
    [DataField("tree", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<NPCConversationTreePrototype>))]
    public string? ConversationTreeId;

    /// <summary>
    /// This is the cached prototype.
    /// </summary>
    [ViewVariables]
    public NPCConversationTreePrototype ConversationTree = default!;

    /// <summary>
    /// Topics that are unlocked in the NPC's conversation tree.
    /// </summary>
    [ViewVariables]
    public HashSet<NPCTopic> UnlockedTopics = new();

    /// <summary>
    /// How long until we stop paying attention to someone for a prompt.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("attentionSpan")]
    public TimeSpan AttentionSpan = TimeSpan.FromSeconds(20);

    /// <summary>
    /// This is the minimum delay before the NPC makes a response.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("delayBeforeResponse")]
    public TimeSpan DelayBeforeResponse = TimeSpan.FromSeconds(0.3);

    /// <summary>
    /// This is the approximate delay per letter typed in text.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("typingDelay")]
    public TimeSpan TypingDelay = TimeSpan.FromSeconds(0.05);

    [ViewVariables]
    public Stack<NPCResponse> ResponseQueue = new();

    /// <summary>
    /// This is when the NPC will respond with its top response.
    /// </summary>
    [ViewVariables]
    [DataField("nextResponse", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextResponse;

    /// <summary>
    /// This is the direction the NPC was facing before looking towards a conversation partner.
    /// </summary>
    [ViewVariables]
    public Angle OriginalFacing;

    /// <summary>
    /// This is who the NPC is paying attention to for conversation.
    /// </summary>
    [ViewVariables]
    public EntityUid? AttendingTo;

    /// <summary>
    /// This is when the NPC will stop paying attention to a specific person.
    /// </summary>
    [ViewVariables]
    [DataField("nextAttentionLoss", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextAttentionLoss;

    /// <summary>
    /// This event is fired the next time the NPC hears something from the
    /// person they're speaking with and it takes control of the response.
    /// </summary>
    [ViewVariables]
    public NPCConversationListenEvent? ListeningEvent;

#region Idle Chatter

    /// <summary>
    /// Whether or not the NPC will say things unprompted.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("idleEnabled")]
    public bool IdleEnabled = true;

    /// <summary>
    /// This is the approximate delay between idle chats.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("idleChatDelay")]
    public TimeSpan IdleChatDelay = TimeSpan.FromMinutes(3);

    /// <summary>
    /// This is the order in which idle chat lines are given.
    /// </summary>
    /// <remarks>
    /// This is randomized both on init and when the lines have been exhausted
    /// to prevent repeating lines twice in a row and to avoid predictable patterns.
    ///
    /// It technically reduces randomness, with the benefit of less repetition.
    /// </remarks>
    [ViewVariables(VVAccess.ReadWrite)]
    public List<int> IdleChatOrder = new();

    /// <summary>
    /// This is the next idle chat line that will be used.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int IdleChatIndex = 0;

    /// <summary>
    /// This is when the NPC will say something out of its list of idle lines.
    /// </summary>
    /// <remarks>
    /// This is reset every time the NPC speaks.
    /// </remarks>
    [ViewVariables]
    [DataField("nextIdleChat", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextIdleChat;

#endregion

}

