using Content.Client.DamageState;
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
        if (!TryGetEntity(args.Hibernator, out var uid) | !TryComp<CanHibernateComponent>(uid, out var comp))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!sprite.TryGetLayer((int) DamageStateVisualLayers.Base, out var layer))
            return;

        layer.SetState(comp!.SpriteStateId);
    }
}
