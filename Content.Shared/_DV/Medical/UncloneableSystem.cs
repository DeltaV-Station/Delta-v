using Content.Shared._DV.Traits.Assorted;
using Content.Shared.Cloning.Events;

namespace Content.Shared._DV.Medical;

public sealed class UncloneableSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UncloneableComponent, CloningAttemptEvent>(OnCloningAttempt);
    }

    private void OnCloningAttempt(Entity<UncloneableComponent> ent, ref CloningAttemptEvent args)
    {
        args.Cancelled = true;
    }
}
