using Content.Client.Eui;
using Content.Shared._DV.Objectives.Eui;
using Content.Shared.Eui;
using Content.Shared.Mind;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Objectives.Eui;

public sealed class ObjectiveEditorEui : BaseEui
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    private readonly ObjectiveEditorUi _editorUi;
    private Entity<MindComponent> _targetMind;

    public ObjectiveEditorEui()
    {
        IoCManager.InjectDependencies(this);

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

        var mind = _entityManager.GetEntity(s.TargetMind);
        if (!_entityManager.TryGetComponent<MindComponent>(mind, out var comp))
        {
            // TODO: Log
            return;
        }

        _targetMind = (mind, comp);

        _editorUi.SetRoleDescription(comp.RoleType, s.Subtype);
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
        SendMessage(new ObjectiveEditorSaveMessage(_editorUi.GetObjectives(), _entityManager.GetNetEntity(_targetMind)));
    }

    private void CreateObjective(EntProtoId? proto)
    {
        SendMessage(new ObjectiveEditorCreateMessage(proto));
    }
}
