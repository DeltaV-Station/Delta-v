using Content.Client.Eui;
using Content.Shared._DV.Objectives.Eui;
using Content.Shared.Eui;

namespace Content.Client._DV.Objectives.Eui;

public sealed class ObjectiveEditorEui : BaseEui
{
    private readonly ObjectiveEditorUi _editorUi;
    private NetEntity _targetMind;

    public ObjectiveEditorEui()
    {
        _editorUi = new ObjectiveEditorUi();
        _editorUi.SaveButton.OnPressed += _ => SaveObjectives();
    }

    private void SaveObjectives()
    {
        SendMessage(new ObjectiveEditorSaveMessage(_editorUi.GetObjectives(), _targetMind));
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
}
