using Content.Shared._DV.Traits.Assorted;
using Content.Shared.Cloning.Events;
using JetBrains.Annotations;

namespace Content.Shared._DV.Medical;

public sealed class UncloneableSystem : EntitySystem
{
    [PublicAPI]
    public bool IsUncloneable(Entity<UncloneableComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        return true;
    }

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
