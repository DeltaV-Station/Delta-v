using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Roles;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;

namespace Content.Server._DV.Objectives.Systems;

/// <summary>
///     Handles picking a random traitor for the kill fellow traitor objective.
/// </summary>
public sealed class PickRandomTraitorSystem : EntitySystem
{
    [Dependency] private readonly PickObjectiveTargetSystem _pickTarget = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PickRandomTraitorComponent, ObjectiveAssignedEvent>(OnRandomTraitorAssigned);
    }

    private void OnRandomTraitorAssigned(Entity<PickRandomTraitorComponent> ent, ref ObjectiveAssignedEvent args)
    {
        _pickTarget.AssignRandomTarget(ent, ref args, mindId =>
            _role.MindHasRole<TraitorRoleComponent>(mindId));
    }
}
