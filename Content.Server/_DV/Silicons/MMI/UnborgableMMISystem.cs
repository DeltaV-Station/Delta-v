using Content.Shared._DV.Traits.Assorted;
using Content.Shared.Interaction.Events;
using Content.Shared.Containers;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;

namespace Content.Server._DV.Silicons.MMI;

public sealed class UnborgableMMISystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<UnborgableMMIComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, UnborgableMMIComponent comp, UseInHandEvent args)
    {
        args.Handled = true;

        if (!TryComp(uid, out ContainerManagerComponent? containers))
            return;

        if (!containers.TryGetContainer(comp.BrainSlotId, out var container) ||
            container.ContainedEntities.Count == 0)
            return;

        var brain = container.ContainedEntities[0];

        if (!HasComp<UnborgableComponent>(brain))
            return;

        args.Handled = false;
    }
}
