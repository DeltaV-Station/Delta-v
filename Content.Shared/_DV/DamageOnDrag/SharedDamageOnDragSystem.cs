using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Damage;
using Robust.Shared.GameObjects;

namespace Content.Shared._DV.DamageOnDrag;

public abstract class SharedDamageOnDragSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageOnDragComponent, MoveEvent>(OnMove);
    }

    private void OnMove(Entity<DamageOnDragComponent> ent, ref MoveEvent args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        var dragging = _mobState.IsIncapacitated(ent);
        if (!dragging)
            return;

        if (TryComp<DamageableComponent>(ent, out var damage) && damage.TotalDamage >= ent.Comp.DamageUpperBound)
            return;

        var factor = (args.NewPosition.Position - args.OldPosition.Position).Length();
        var normalDamage = ent.Comp.Damage * factor;

        _damageable.TryChangeDamage(ent, normalDamage);
        HandleBloodstreamDamage(ent, ref args);
    }

    protected virtual void HandleBloodstreamDamage(Entity<DamageOnDragComponent> ent, ref MoveEvent args)
    {
    }
}
