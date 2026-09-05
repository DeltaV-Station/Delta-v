using Content.Shared.Mind.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;

namespace Content.Shared._DV.Uprising;

public sealed class CrewConversionObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrewConversionObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<CrewConversionObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var total = 0;
        var counted = 0;

        var query = AllEntityQuery<MindContainerComponent, NpcFactionMemberComponent>();
        while (query.MoveNext(out var uid, out var mind, out var faction))
        {
            if (mind.Mind is not { } mindUid)
                continue;

            if (!_npcFaction.IsMemberOfAny((uid, faction), ent.Comp.ConversionSourceFactions))
                continue;

            total++;

            foreach (var role in ent.Comp.ConvertedRoles)
            {
                if (!EntityManager.ComponentFactory.TryGetRegistration(role, out var roleReg))
                {
                    Log.Error($"Role component not found for CrewConversionObjective: {role}");
                    continue;
                }

                if (!_role.MindHasRole(mindUid, roleReg.Type, out _))
                    continue;

                counted++;
                break;
            }
        }

        if (total == 0)
            return;

        args.Progress = ((float)counted / (float)total) / ent.Comp.TargetFraction;
    }
}
