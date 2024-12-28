using Content.Server.DeltaV.Objectives.Components;
using Content.Server.Revolutionary.Components;
using Content.Shared.Cuffs.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server.DeltaV.Objectives.Systems;

public sealed class KidnapHeadsConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KidnapHeadsConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<KidnapHeadsConditionComponent> condition, ref ObjectiveGetProgressEvent args)
    {
        var allHumans = _mind.GetAliveHumans();
        var totalHeads = 0;
        var cuffedHeads = 0;
        foreach (var human in allHumans)
        {
            if (!HasComp<CommandStaffComponent>(human.Comp.OwnedEntity))
                continue;
            totalHeads++;
            if (!(TryComp<CuffableComponent>(human.Comp.OwnedEntity, out var cuffableComp) && cuffableComp.CuffedHandCount > 0))
                continue;
            cuffedHeads++;
        }

        if (totalHeads == 0)
            args.Progress = 1.0f;
        else if (totalHeads < condition.Comp.NumberOfHeadsToKidnap)
            args.Progress = cuffedHeads / totalHeads;
        else
            args.Progress = cuffedHeads / condition.Comp.NumberOfHeadsToKidnap;
    }

}

