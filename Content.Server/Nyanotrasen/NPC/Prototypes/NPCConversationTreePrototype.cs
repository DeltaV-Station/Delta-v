using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Server.NPC.Events;

namespace Content.Server.NPC.Prototypes;

[Prototype("npcConversationTree")]
public sealed class NPCConversationTreePrototype : IPrototype, ISerializationHooks
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// Dialogue contains all the topics to which an NPC can discuss.
    /// </summary>
    [ViewVariables]
    [DataField("dialogue", required: true)]
    public readonly NPCTopic[] Dialogue = default!;

    /// <summary>
    /// Attention responses are what the NPC says when they start paying
    /// attention to you without a specific question or prompt to respond to.
    /// </summary>
    [ViewVariables]
    [DataField("attention", required: true)]
    public readonly NPCResponse[] Attention = default!;

    /// <summary>
    /// Idle responses are just things the NPC will say when nothing else is
    /// going on, after some time.
    /// </summary>
    [ViewVariables]
    [DataField("idle", required: true)]
    public readonly NPCResponse[] Idle = default!;

    /// <summary>
    /// Unknown responses are what the NPC says when they can't respond to a
    /// particular question or prompt.
    /// </summary>
    [ViewVariables]
    [DataField("unknown", required: true)]
    public readonly NPCResponse[] Unknown = default!;

    /// <summary>
    /// Custom responses are available to use in extensions to the NPC
    /// Conversation system.
    /// </summary>
    // NOTE: This may be removed in favor of storing NPCResponses on custom
    // components, i.e. an NPCShopkeeperComponent, but for now, it lives here
    // to help demonstrate some features.
    [ViewVariables]
    [DataField("custom")]
    public readonly Dictionary<string, NPCResponse[]> Custom = default!;

    /// <summary>
    /// This exists as a quick way to map a prompt to a topic.
    /// </summary>
    public readonly Dictionary<string, NPCTopic> PromptToTopic = new();

    // ISerializationHooks _is_ obsolete, but ConstructionGraphPrototype is using it as of this commit,
    // and I'm not quite sure how to otherwise do this.
    //
    // I will look at that prototype when ISerializationHooks is phased out.
    void ISerializationHooks.AfterDeserialization()
    {
        // Cache the strings mapping to prompts.
        foreach (var topic in Dialogue)
        {
            foreach (var prompt in topic.Prompts)
            {
                PromptToTopic[prompt] = topic;
            }
        }
    }
}

[DataDefinition]
public sealed class NPCTopic
{
    [DataField("prompts")]
    public readonly string[] Prompts = default!;

    /// <summary>
    /// This determines the likelihood of this topic being selected over any
    /// other, given the existence of multiple candidates.
    /// </summary>
    [DataField("weight")]
    public readonly float Weight = 1.0f;

    /// <summary>
    /// Locked topics will not be accessible through dialogue until unlocked.
    /// </summary>
    [DataField("locked")]
    public readonly bool Locked;

    /// <summary>
    /// Hidden topics won't show up in any form of "help" question.
    /// </summary>
    [DataField("hidden")]
    public readonly bool Hidden;

    [DataField("responses", required: true)]
    public readonly NPCResponse[] Responses = default!;
}

[DataDefinition]
public sealed class NPCResponse
{
    public NPCResponse() { }

    public NPCResponse(string? text, SoundSpecifier? audio = null, NPCConversationEvent? ev = null)
    {
        Text = text;
        Audio = audio;
        Event = ev;
    }

    public override string ToString()
    {
        return $"NPCResponse({Text})";
    }

    [DataField("text")]
    public readonly string? Text;

    [DataField("audio")]
    public readonly SoundSpecifier? Audio;

    /* [DataField("emote")] */
    /* public string? Emote; */

    /// <summary>
    /// This event is raised when the response is queued,
    /// for the purpose of dynamic responses.
    /// </summary>
    [DataField("is")]
    public readonly NPCConversationEvent? Is;

    /// <summary>
    /// This event is raised after the response is made.
    /// </summary>
    [DataField("event")]
    public readonly NPCConversationEvent? Event;

    /// <summary>
    /// This event is raised when the NPC next hears a response,
    /// allowing the response to be processed by other systems.
    /// </summary>
    [DataField("listenEvent")]
    public readonly NPCConversationListenEvent? ListenEvent;
}

