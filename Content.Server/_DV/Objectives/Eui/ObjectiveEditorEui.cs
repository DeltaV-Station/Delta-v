using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Server.Objectives;
using Content.Shared._DV.Objectives.Eui;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Utility;

namespace Content.Server._DV.Objectives.Eui;

public sealed class ObjectiveEditorEui(ObjectivesSystem objectiveSystem, EntityManager entityManager, IAdminManager manager) : BaseEui
{
    private readonly ObjectivesSystem _objectiveSystem = objectiveSystem;
    private readonly EntityManager _entityManager = entityManager;
    private readonly IAdminManager _adminManager = manager;
    private readonly ISawmill _sawmill = Logger.GetSawmill("objective-editor-eui");
    private readonly Dictionary<string, List<ObjectiveData>> _objectives = [];
    private EntityUid _targetMind;

    public override EuiStateBase GetNewState()
    {
        return new ObjectiveEditorEUIState(_objectives, _entityManager.GetNetEntity(_targetMind));
    }

    public void UpdateObjectives(Entity<MindComponent> mind)
    {
        if (!IsAllowed())
            return;

        foreach (var objective in mind.Comp.Objectives)
        {
            var info = _objectiveSystem.GetInfo(objective, mind, mind.Comp);
            if (!info.HasValue)
                continue;

            var metadata = _entityManager.GetComponent<MetaDataComponent>(objective);
            var data = new ObjectiveData(metadata.EntityPrototype?.ID, info.Value);

            var issuer = _entityManager.GetComponent<ObjectiveComponent>(objective).LocIssuer;
            _objectives.GetOrNew(issuer).Add(data);
        }

        _targetMind = mind.Owner;
        StateDirty();
    }

    private bool IsAllowed()
    {
        var adminData = _adminManager.GetAdminData(Player);
        if (adminData == null || !adminData.HasFlag(AdminFlags.Moderator))
        {
            _sawmill.Warning($"Player {Player.UserId} tried to open / use player objective editor UI without permission.");
            return false;
        }

        return true;
    }
}
