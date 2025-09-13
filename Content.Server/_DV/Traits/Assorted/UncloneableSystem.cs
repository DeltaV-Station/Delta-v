using Content.Shared.Cloning.Events;
using Content.Shared.Traits.Assorted;

namespace Content.Server.Traits.Assorted;

public sealed class UncloneableSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UncloneableComponent, CloningAttemptEvent>(OnCloningAttempt);
    }

    private void OnCloningAttempt(Entity<UncloneableComponent> ent, ref CloningAttemptEvent args)
    {
        if (!ent.Comp.Cloneable)
            args.Cancelled = true;
    }
}
