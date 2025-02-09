using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.CustomObjectiveSummery;

/// <summary>
///     Message from the client with what they are updating their summery to.
/// </summary>
public sealed class CustomObjectiveClientSetObjective : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    /// <summary>
    ///     The summery that the user wrote.
    /// </summary>
    public string Summery = string.Empty;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Summery = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Summery);
    }

    public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.ReliableUnordered;
}

/// <summary>
///     Clients listen for this event and when they get it, they open a popup so the player can fill out the objective summery.
/// </summary>
[Serializable, NetSerializable]
public sealed class CustomObjectiveSummeryOpenMessage : EntityEventArgs;

/// <summary>
///     DeltaV event for when the evac shuttle leaves.
/// </summary>
[Serializable, NetSerializable]
public sealed class EvacShuttleLeftEvent : EventArgs;
