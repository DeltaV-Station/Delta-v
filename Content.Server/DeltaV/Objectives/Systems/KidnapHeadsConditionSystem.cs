using Content.Server.DeltaV.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Revolutionary.Components;
using Content.Shared.Cuffs.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server.DeltaV.Objectives.Systems;

public sealed class KidnapHeadsConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly NumberObjectiveSystem _numberObjectiveSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KidnapHeadsConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<KidnapHeadsConditionComponent> condition, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetProgress(condition);
    }

    public float GetProgress(Entity<KidnapHeadsConditionComponent> condition)
    {
        GetTotalAndCuffedHeads(out var totalHeads, out var cuffedHeads);

        if (totalHeads == 0)
            return 1.0f;

        return (float) cuffedHeads / Math.Min(totalHeads, _numberObjectiveSystem.GetTarget(condition));
    }

    public bool IsCompleted(Entity<KidnapHeadsConditionComponent> condition)
    {
        GetTotalAndCuffedHeads(out var totalHeads, out var cuffedHeads);
        if (totalHeads == 0)
            return false;

        return cuffedHeads == Math.Min(totalHeads, _numberObjectiveSystem.GetTarget(condition));
    }

    private void GetTotalAndCuffedHeads(out int totalHeads, out int cuffedHeads)
    {
        var allHumans = _mind.GetAliveHumans();
        totalHeads = 0;
        cuffedHeads = 0;
        foreach (var human in allHumans)
        {
            if (!HasComp<CommandStaffComponent>(human.Comp.OwnedEntity))
                continue;
            totalHeads++;

            if (!(TryComp<CuffableComponent>(human.Comp.OwnedEntity, out var cuffableComp) && cuffableComp.CuffedHandCount > 0))
                continue;
            cuffedHeads++;
        }
    }
}

