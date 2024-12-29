using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.FeedbackOverwatch;

/// <summary>
///     When clients recieve this message a popup will appear on their screen with the contents from the given prototype.
/// </summary>
[Serializable, NetSerializable]
public sealed class FeedbackPopupMessage : EntityEventArgs
{
    public ProtoId<FeedbackPopupPrototype> FeedbackPrototype;

    public FeedbackPopupMessage(ProtoId<FeedbackPopupPrototype> feedbackPrototype)
    {
        FeedbackPrototype = feedbackPrototype;
    }
};
