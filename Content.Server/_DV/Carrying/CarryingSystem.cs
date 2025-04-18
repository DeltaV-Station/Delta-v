using Content.Server.Polymorph.Systems;
using Content.Shared._DV.Carrying;
using Content.Shared.Mind.Components;

namespace Content.Server._DV.Carrying;

public sealed class CarryingSystem : SharedCarryingSystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<CarryingComponent, BeforePolymorphedEvent>(OnBeforePolymorphed);

        base.Initialize();
    }

    private void OnBeforePolymorphed(Entity<CarryingComponent> ent, ref BeforePolymorphedEvent args)
    {
        if (HasComp<MindContainerComponent>(ent.Comp.Carried))
            DropCarried(ent, ent.Comp.Carried);
    }
}
