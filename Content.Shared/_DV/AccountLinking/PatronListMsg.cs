using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.AccountLinking;

public sealed class PatronListMsg : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;

    public List<SharedPatron> Patrons = default!;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var count = buffer.ReadVariableInt32();
        Patrons = new List<SharedPatron>(count);
        for (var i = 0; i < count; i++)
        {
            var name = buffer.ReadString();
            var tier = buffer.ReadString();
            Patrons.Add(new SharedPatron(name, tier));
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(Patrons.Count);
        foreach (var patron in Patrons)
        {
            buffer.Write(patron.Name);
            buffer.Write(patron.Tier);
        }
    }
}
