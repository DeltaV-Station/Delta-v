using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Shared._DV.Objectives.Eui;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Utility;

namespace Content.Server._DV.Objectives.Eui;

public sealed class ObjectiveEditorEui(EntityManager entityManager, IAdminManager manager) : BaseEui
{
    private readonly EntityManager _entityManager = entityManager;
    private readonly ObjectivesSystem _objectiveSystem = entityManager.System<ObjectivesSystem>();
    private readonly MindSystem _mind = entityManager.System<MindSystem>();
    private readonly MetaDataSystem _metadata = entityManager.System<MetaDataSystem>();
    private readonly IAdminManager _adminManager = manager;
    private readonly ISawmill _sawmill = Logger.GetSawmill("objective-editor-eui");
    private readonly Dictionary<string, List<ObjectiveData>> _objectives = [];
    private Entity<MindComponent> _targetMind;

    public override EuiStateBase GetNewState()
    {
        return new ObjectiveEditorEUIState(
            _objectives,
            _entityManager.GetNetEntity(_targetMind.Owner),
            _targetMind.Comp.RoleType,
            _targetMind.Comp.Subtype);
    }

    public void UpdateObjectives(Entity<MindComponent> mind)
    {
        if (!IsAllowed())
            return;

        _targetMind = mind;
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

        StateDirty();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not ObjectiveEditorSaveMessage message)
            return;

        if (!IsAllowed())
            return;

        // Just remove the first objective from this mind until we're empty
        while (_targetMind.Comp.Objectives.Count > 0)
        {
            _mind.TryRemoveObjective(_targetMind, _targetMind.Comp, 0);
        }

        // Now spawn a new set of objectives to match
        foreach (var (issuer, objectives) in message.Objectives)
        {
            foreach (var objective in objectives)
            {
                var newObjective = CreateObjective(objective);
                if (!newObjective.HasValue)
                    continue; // TODO(Barry) Log an error

                var metadata = _entityManager.GetComponent<MetaDataComponent>(newObjective.Value);

                // Setup new Title/Description
                _metadata.SetEntityName(newObjective.Value, objective.Info.Title, metadata: metadata);
                _metadata.SetEntityDescription(newObjective.Value, objective.Info.Description, metadata: metadata);

                // Now the issuer
                _objectiveSystem.SetIssuer(newObjective.Value, issuer);

                _mind.AddObjective(_targetMind.Owner, _targetMind.Comp, newObjective.Value);
            }
        }
    }

    private EntityUid? CreateObjective(ObjectiveData data)
    {
        EntityUid? objective = null;
        if (data.Proto.HasValue)
            objective = _objectiveSystem.TryCreateObjective(_targetMind, _targetMind.Comp, data.Proto.Value);

        if (!objective.HasValue)
            objective = _objectiveSystem.TryCreateObjective(_targetMind, _targetMind.Comp, "");

        return objective;
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
