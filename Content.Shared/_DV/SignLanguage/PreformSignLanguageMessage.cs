using Content.Shared._DV.SignLanguage.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.SignLanguage;

/// <summary>
/// Message sent when a player performs a sign language gesture.
/// </summary>
[Serializable, NetSerializable]
public sealed class PerformSignLanguageMessage(
    ProtoId<SignTopicPrototype> topicId,
    ProtoId<SignEventPrototype> eventId,
    ProtoId<SignIntentPrototype> intentId,
    ProtoId<SignIntensityPrototype>? intensityId = null)
    : EntityEventArgs
{
    public ProtoId<SignTopicPrototype> TopicId { get; } = topicId;
    public ProtoId<SignEventPrototype> EventId { get; } = eventId;
    public ProtoId<SignIntentPrototype> IntentId { get; } = intentId;
    public ProtoId<SignIntensityPrototype>? IntensityId { get; } = intensityId;
}
