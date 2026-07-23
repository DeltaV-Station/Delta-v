using Content.Shared.Eui;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Objectives.Eui;

[Serializable, NetSerializable]
public sealed class ObjectiveEditorEUIState(
    List<ObjectiveData> objectives,
    NetEntity targetMind,
    LocId? subtype) : EuiStateBase
{
    public List<ObjectiveData> Objectives { get; } = objectives;
    public NetEntity TargetMind { get; } = targetMind;

    public LocId? Subtype = subtype;
}

[Serializable, NetSerializable]
public sealed class ObjectiveEditorSaveMessage(
   List<ObjectiveData> objectives, NetEntity target, bool silent) : EuiMessageBase
{
    public List<ObjectiveData> Objectives = objectives;
    public NetEntity Target = target;
    public bool Silent = silent;
}


[Serializable, NetSerializable]
public sealed class ObjectiveEditorCreateMessage(EntProtoId? proto = null) : EuiMessageBase
{
    public EntProtoId? Proto = proto;
}


[Serializable, NetSerializable]
public sealed class ObjectiveEditorCreateResponse(ObjectiveData? data) : EuiMessageBase
{
    public ObjectiveData? Data = data;
}
