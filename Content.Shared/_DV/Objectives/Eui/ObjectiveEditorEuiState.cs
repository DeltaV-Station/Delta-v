using Content.Shared.Eui;
using Content.Shared.Mind;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Objectives.Eui;

[Serializable, NetSerializable]
public sealed class ObjectiveEditorEUIState(
    Dictionary<string, List<ObjectiveData>> objectives,
    NetEntity targetMind,
    ProtoId<RoleTypePrototype>? role,
    LocId? subtype) : EuiStateBase
{
    public Dictionary<string, List<ObjectiveData>> Objectives { get; } = objectives;
    public NetEntity TargetMind { get; } = targetMind;

    public ProtoId<RoleTypePrototype>? Role = role;
    public LocId? Subtype = subtype;
}

[Serializable, NetSerializable]
public sealed class ObjectiveEditorSaveMessage(
    List<ObjectiveData> objectives, NetEntity target,
    ProtoId<RoleTypePrototype>? role,
    LocId? subtype) : EuiMessageBase
{
    public List<ObjectiveData> Objectives = objectives;
    public NetEntity Target = target;
    public ProtoId<RoleTypePrototype>? Role = role;
    public LocId? Subtype = subtype;
}
