using Content.Shared.Hands;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Augments;

public sealed class SharedAugmentToolPanelSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentToolPanelActiveItemComponent, ContainerGettingRemovedAttemptEvent>(OnDropAttempt);
    }

    private void OnDropAttempt(Entity<AugmentToolPanelActiveItemComponent> ent, ref ContainerGettingRemovedAttemptEvent args)
    {
        args.Cancel();
    }
}
