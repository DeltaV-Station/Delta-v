using Content.Shared.DeltaV.SpaceFerret;
using Robust.Client.GameObjects;

namespace Content.Client.DeltaV.SpaceFerret;

public sealed class CanHibernateSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<EntityHasHibernated>(OnHibernateEvent);
    }

    public void OnHibernateEvent(EntityHasHibernated args)
    {
        if (!TryGetEntity(args.Hibernator, out var uid))
        {
            return;
        }

        if (!TryComp<SpriteComponent>(uid, out var comp))
        {
            return;
        }

        comp.LayerSetState(0, args.SpriteStateId);
    }
}
