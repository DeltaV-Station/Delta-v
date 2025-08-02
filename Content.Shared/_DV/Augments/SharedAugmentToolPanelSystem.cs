using Content.Shared.Hands;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Augments;

public abstract class SharedAugmentToolPanelSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentToolPanelComponent, AugmentGetDrawEvent>(OnGetDraw);

        SubscribeLocalEvent<AugmentToolPanelActiveItemComponent, ContainerGettingRemovedAttemptEvent>(OnDropAttempt);
    }

    private void OnGetDraw(Entity<AugmentToolPanelComponent> ent, ref AugmentGetDrawEvent args)
    {
        if (ent.Comp.Active)
            args.Add(ent.Comp.ActiveDraw);
    }

    private void OnDropAttempt(Entity<AugmentToolPanelActiveItemComponent> ent, ref ContainerGettingRemovedAttemptEvent args)
    {
        args.Cancel();
    }
}
