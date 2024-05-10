using Content.Shared.Abilities.Psionics;
using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

public sealed class BecomePsionicConditionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BecomePsionicConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<BecomePsionicConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetProgress(args.Mind);
    }

    private float GetProgress(MindComponent mind)
    {
        return HasComp<PsionicComponent>(mind.OwnedEntity) ? 1 : 0;
    }
}
