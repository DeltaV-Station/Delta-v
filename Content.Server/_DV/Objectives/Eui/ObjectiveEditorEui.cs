using Content.Server.Administration.Managers;

using Content.Server.EUI;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Shared.Objectives;
using Content.Shared._DV.Objectives.Eui;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server._DV.Objectives.Eui;

public sealed class ObjectiveEditorEui : BaseEui
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;

    private readonly ObjectivesSystem _objectiveSystem = default!;
    private readonly MindSystem _mind = default!;
    private readonly MetaDataSystem _metadata = default!;

    private readonly ISawmill _sawmill = Logger.GetSawmill("objective-editor-eui"); // TODO(Barry): Actually need this or?

    private readonly Dictionary<EntityUid, ObjectiveData> _objectives = [];
    private readonly EntProtoId _fallbackObjective = "EditorDefaultObjective";
    private readonly ObjectiveInfo _fallbackData = new ObjectiveInfo(
        "Title",
        "Description",
        new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Nano/cross.svg.png")),
        0f
    );

    private Entity<MindComponent> _targetMind;

    /// <summary>
    /// A list of temporary objective entities that have not yet been
    /// added to the target's mind.
    /// </summary>
    private List<EntityUid> _temporaryIds = [];

    public ObjectiveEditorEui()
    {
        IoCManager.InjectDependencies(this);

        _objectiveSystem = _entityManager.System<ObjectivesSystem>();
        _mind = _entityManager.System<MindSystem>();
        _metadata = _entityManager.System<MetaDataSystem>();
    }

    public override EuiStateBase GetNewState()
    {
        return new ObjectiveEditorEUIState(
            [.. _objectives.Values],
            _entityManager.GetNetEntity(_targetMind.Owner),
            _targetMind.Comp.Subtype);
    }

    public override void Closed()
    {
        base.Closed();

        ClearTempObjectives();
    }

    /// <summary>
    /// Grabs objectives for the specified mind and sets the UI state to reflect it.
    /// </summary>
    /// <param name="mind">The target mind to grab objectives for.</param>
    public void UpdateObjectivesFor(Entity<MindComponent> mind)
    {
        if (!IsAllowed())
            return;

        _targetMind = mind;
        foreach (var objective in mind.Comp.Objectives)
        {
            _objectives.Add(objective, CreateObjectiveData(objective, mind));
        }

        ClearTempObjectives();
        StateDirty();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (!IsAllowed())
            return;

        switch (msg)
        {
            case ObjectiveEditorSaveMessage message:
                HandleSaveMessage(message);
                break;
            case ObjectiveEditorCreateMessage message:
                HandleCreateMessage(message);
                break;
        }
    }

    /// <summary>
    /// Handle when a user on the client side wishes to save their current objective changes.
    /// </summary>
    /// <param name="message">The save message, which includes the new objectives.</param>
    private void HandleSaveMessage(ObjectiveEditorSaveMessage message)
    {
        var oldObjectives = _objectives.ShallowClone();
        _objectives.Clear(); // Clean up the previous set we knew about

        foreach (var incomingObjective in message.Objectives)
        {
            var objEnt = _entityManager.GetEntity(incomingObjective.Entity);

            EntityUid objective;
            if (oldObjectives.ContainsKey(objEnt))
            {
                // We can just update this entity, no need to add it to the mind either
                objective = objEnt;
                oldObjectives.Remove(objEnt); // Ensure it's not cleaned up
            }
            else if (_temporaryIds.Contains(objEnt))
            {
                objective = objEnt;
                _temporaryIds.Remove(objEnt); // Ensure it's not cleaned up
                _mind.AddObjective(_targetMind.Owner, _targetMind.Comp, objective);
            }
            else
            {
                // Gotta make a new entity for it
                var newObjective = CreateObjective(incomingObjective.Proto);
                if (!newObjective.HasValue)
                    continue; // TODO(Barry) Log an error
                objective = newObjective.Value;

                // Ensure we add the new objective we've just created
                _mind.AddObjective(_targetMind.Owner, _targetMind.Comp, objective);
            }

            var metadata = _entityManager.GetComponent<MetaDataComponent>(objective);

            // Setup new Title/Description
            _metadata.SetEntityName(objective, incomingObjective.Info.Title, metadata: metadata);
            _metadata.SetEntityDescription(objective, incomingObjective.Info.Description, metadata: metadata);

            // Now the issuer and Icon
            _objectiveSystem.SetIssuer(objective, incomingObjective.Issuer);
            _objectiveSystem.SetIcon(objective, incomingObjective.Info.Icon);

            _objectives.Add(objective, CreateObjectiveData(objective, _targetMind));
        }

        foreach (var old in oldObjectives.Keys)
        {
            _mind.TryRemoveObjective(_targetMind, _targetMind.Comp, old);
        }

        ClearTempObjectives(); // Ensure all temporary objectives are cleared and deleted
        StateDirty(); // Ensure client is freshly updated with all the new Objective information
    }

    /// <summary>
    /// Handles when a user on the client side wants to create a whole new objective.
    /// The new objective entity will be stored temporarily until either the window closes,
    /// or the user performs a save.
    /// </summary>
    /// <param name="message">The create message, which may include a ProtoId to use.</param>
    private void HandleCreateMessage(ObjectiveEditorCreateMessage message)
    {
        var newObjective = CreateObjective(message.Proto);
        if (!newObjective.HasValue)
            return;

        // N.b. The objective is not yet added to the mind.
        var data = CreateObjectiveData(newObjective.Value, _targetMind);
        _temporaryIds.Add(newObjective.Value);
        SendMessage(new ObjectiveEditorCreateResponse(data));
    }

    /// <summary>
    /// Creates an ObjectiveData structure for the specified objective entity.
    /// </summary>
    /// <param name="objective">The objective entity.</param>
    /// <param name="mind">The mind the objective is bound to.</param>
    /// <param name="data">The output ObjectiveData structure.</param>
    private ObjectiveData CreateObjectiveData(
        EntityUid objective,
        Entity<MindComponent> mind)
    {
        var info = _objectiveSystem.GetInfo(objective, mind, mind.Comp);
        if (!info.HasValue)
            info = _fallbackData;

        var metadata = _entityManager.GetComponent<MetaDataComponent>(objective);
        var issuer = _entityManager.GetComponent<ObjectiveComponent>(objective).LocIssuer;

        return new ObjectiveData(
            _entityManager.GetNetEntity(objective),
            issuer,
            metadata.EntityPrototype?.ID,
            info.Value);
    }

    /// <summary>
    /// Clears all temporary objectives and queues them for deletion.
    /// </summary>
    private void ClearTempObjectives()
    {
        foreach (var ent in _temporaryIds)
        {
            _entityManager.QueueDeleteEntity(ent);
        }
    }

    /// <summary>
    /// Creates a new entity matching the specified Entity Prototype or a fallback, if that fails.
    /// </summary>
    /// <param name="prototype">The prototype to use, may be null.</param>
    /// <returns>The EntityUid of the new objective if successful, otherwise null.</returns>
    private EntityUid? CreateObjective(EntProtoId? prototype)
    {
        EntityUid? objective = null;
        if (prototype.HasValue)
            objective = _objectiveSystem.TryCreateObjective(_targetMind, _targetMind.Comp, prototype.Value);

        if (!objective.HasValue)
            objective = _objectiveSystem.TryCreateObjective(_targetMind, _targetMind.Comp, _fallbackObjective);

        return objective;
    }

    /// <summary>
    /// Checks whether the current user is allowed to edit the objectives of any mind.
    /// </summary>
    /// <returns>True if allowed, otherwise False.</returns>
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
