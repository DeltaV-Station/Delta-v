using Content.Client.Eui;
using Content.Shared._DV.Objectives.Eui;
using Content.Shared.Eui;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Objectives.Eui;

public sealed class ObjectiveEditorEui : BaseEui
{
    private readonly ObjectiveEditorUi _editorUi;
    private NetEntity _targetMind;

    public ObjectiveEditorEui()
    {
        _editorUi = new ObjectiveEditorUi();
        _editorUi.SaveAction += SaveObjectives;
        _editorUi.CreateAction += CreateObjective;
    }

    public override void Opened()
    {
        base.Opened();
        _editorUi.OpenCentered();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not ObjectiveEditorEUIState s)
            return;

        _targetMind = s.TargetMind;
        _editorUi.SetRoleDescription(s.Role, s.Subtype);
        _editorUi.SetObjectives(s.Objectives);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            case ObjectiveEditorCreateResponse message:
                HandleCreateResponse(message);
                break;
        }
    }

    private void HandleCreateResponse(ObjectiveEditorCreateResponse response)
    {
        if (response.Data == null)
        {
            // TODO(Barry): Logging
            return;
        }

        _editorUi.AddObjective(response.Data);
    }

    private void SaveObjectives()
    {
        SendMessage(new ObjectiveEditorSaveMessage(_editorUi.GetObjectives(), _targetMind));
    }

    private void CreateObjective(EntProtoId? proto)
    {
        SendMessage(new ObjectiveEditorCreateMessage(proto));
    }
}
