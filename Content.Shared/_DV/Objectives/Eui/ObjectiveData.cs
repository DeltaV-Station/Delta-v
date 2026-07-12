using Content.Shared.Objectives;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Objectives.Eui;

[Serializable, NetSerializable]
public sealed class ObjectiveData(NetEntity entity, string issuer, EntProtoId? proto, ObjectiveInfo info)
{
    public NetEntity Entity = entity;
    public string Issuer = issuer;
    public EntProtoId? Proto = proto;
    public ObjectiveInfo Info = info;
}
