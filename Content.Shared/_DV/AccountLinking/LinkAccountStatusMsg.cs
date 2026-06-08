using System.IO;
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.AccountLinking;

public sealed class LinkAccountStatusMsg : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;

    public SharedPatronFull? Patron;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var isPatron = buffer.ReadBoolean();
        if (!isPatron)
            return;

        buffer.ReadPadBits();
        var length = buffer.ReadVariableInt32();
        using var stream = new MemoryStream(length);
        buffer.ReadAlignedMemory(stream, length);
        Patron = serializer.Deserialize<SharedPatronFull>(stream);
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        if (Patron == null)
        {
            buffer.Write(false);
            return;
        }

        buffer.Write(true);
        buffer.WritePadBits();
        using (var stream = new MemoryStream())
        {
            serializer.Serialize(stream, Patron);
            buffer.WriteVariableInt32((int) stream.Length);
            stream.TryGetBuffer(out var segment);
            buffer.Write(segment);
        }
    }
}
