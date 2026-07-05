using Content.Shared.Objectives;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Objectives.Eui;

[Serializable, NetSerializable]
public sealed class ObjectiveData(EntProtoId? proto, ObjectiveInfo info)
{
    public EntProtoId? Proto = proto;
    public ObjectiveInfo Info = info;
}
