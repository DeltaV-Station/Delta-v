using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Revolutionary.Components;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server._DV.Objectives.Systems;

public sealed class KidnapHeadsConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly NumberObjectiveSystem _number = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffable = default!;

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

        return (float) cuffedHeads / Math.Min(totalHeads, _number.GetTarget(condition));
    }

    public bool IsCompleted(Entity<KidnapHeadsConditionComponent> condition)
    {
        GetTotalAndCuffedHeads(out var totalHeads, out var cuffedHeads);
        if (totalHeads == 0)
            return false;

        return cuffedHeads == Math.Min(totalHeads, _number.GetTarget(condition));
    }

    private void GetTotalAndCuffedHeads(out int totalHeads, out int cuffedHeads)
    {
        var allHumanMinds = _mind.GetAliveHumans();
        totalHeads = 0;
        cuffedHeads = 0;
        foreach (var mind in allHumanMinds)
        {
            if (mind.Comp.OwnedEntity is not { } mob)
                continue;

            if (!HasComp<CommandStaffComponent>(mob))
                continue;
            totalHeads++;

            if (!TryComp<CuffableComponent>(mob, out var cuffable) || !_cuffable.IsCuffed((mob, cuffable)))
                continue;
            cuffedHeads++;
        }
    }
}
