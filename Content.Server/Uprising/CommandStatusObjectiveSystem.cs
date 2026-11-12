using Content.Server.Revolutionary.Components;
using Content.Shared._DV.Uprising;
using Content.Shared.Cuffs.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;
using Content.Shared.Station;

namespace Content.Server.Uprising;

public sealed class CommandStatusObjectiveSystem : EntitySystem
{
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CommandStatusObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private bool CheckIncapacitated(EntityUid uid)
    {
        if (TryComp<CuffableComponent>(uid, out var cuffed) && cuffed.CuffedHandCount > 0)
            return true;

        if (!TryComp<MobStateComponent>(uid, out var state))
            return true;

        if (state.CurrentState is MobState.Dead or MobState.Invalid)
            return true;

        if (_station.GetOwningStation(uid) == null)
            return true;

        return false;
    }

    private void OnGetProgress(Entity<CommandStatusObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var total = 0;
        var counted = 0;

        var query = AllEntityQuery<MindContainerComponent, CommandStaffComponent>();
        while (query.MoveNext(out var uid, out var mind, out _))
        {
            if (mind.Mind is not { } mindUid)
                continue;

            total++;

            var converted = false;
            foreach (var role in ent.Comp.ConvertedRoles)
            {
                if (!EntityManager.ComponentFactory.TryGetRegistration(role, out var roleReg))
                {
                    Log.Error($"Role component not found for CrewConversionObjective: {role}");
                    continue;
                }

                if (!_role.MindHasRole(mindUid, roleReg.Type, out _))
                    continue;

                converted = true;
                break;
            }

            var incapacitated = CheckIncapacitated(uid);

            if (converted && !incapacitated || !converted && ent.Comp.ShouldUnconvertedBeIncapacitated && incapacitated)
            {
                counted++;
            }
        }

        if (total == 0)
            return;

        args.Progress = (float)counted / (float)total;
    }
}
