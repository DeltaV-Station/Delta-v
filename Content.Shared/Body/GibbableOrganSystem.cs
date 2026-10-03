using Content.Shared.Gibbing;

namespace Content.Shared.Body;

public sealed class GibbableOrganSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GibbableOrganComponent, BodyRelayedEvent<BeingGibbedEvent>>(OnBeingGibbed);
    }

    private void OnBeingGibbed(Entity<GibbableOrganComponent> ent, ref BodyRelayedEvent<BeingGibbedEvent> args)
    {
        // Begin DeltaV - gibbable activation
        if (ent.Comp.Active)
            args.Args.Giblets.Add(ent);
        // End DeltaV - gibbable activation
    }
}
