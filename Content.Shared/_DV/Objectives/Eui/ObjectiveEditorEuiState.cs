using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Objectives.Eui;

[Serializable, NetSerializable]
public sealed class ObjectiveEditorEUIState(Dictionary<string, List<ObjectiveData>> objectives, NetEntity targetMind) : EuiStateBase
{
    public Dictionary<string, List<ObjectiveData>> Objectives { get; } = objectives;
    public NetEntity TargetMind { get; } = targetMind;
}

[Serializable, NetSerializable]
public sealed class ObjectiveEditorSaveMessage(List<ObjectiveData> objectives, NetEntity target) : EuiMessageBase
{
    public List<ObjectiveData> Objectives = objectives;
    public NetEntity Target = target;
}
