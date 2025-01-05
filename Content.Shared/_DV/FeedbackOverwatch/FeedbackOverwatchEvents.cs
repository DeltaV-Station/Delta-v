using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.FeedbackOverwatch;

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
}

/// <summary>
///     Stores a users response to feedback.
/// </summary>
public sealed class FeedbackResponseMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    /// <summary>
    ///     The feedback that the user is sending.
    /// </summary>
    public string FeedbackName = string.Empty;

    /// <summary>
    ///     The feedback that the user is sending.
    /// </summary>
    public string FeedbackMessage = string.Empty;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        FeedbackName = buffer.ReadString();
        FeedbackMessage = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(FeedbackName);
        buffer.Write(FeedbackMessage);
    }

    public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.ReliableUnordered;
}
