using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared._DV.DamageOnDrag;
using Content.Shared.Damage;
using Robust.Shared.GameObjects;

namespace Content.Shared._DV.DamageOnDrag;

public class DamageOnDragSystem : SharedDamageOnDragSystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private BloodstreamSystem _bloodstream = default!;

    protected override void HandleBloodstreamDamage(Entity<DamageOnDragComponent> ent, ref MoveEvent args)
    {
        if (!TryComp<BloodstreamComponent>(ent, out var bloodstream) || bloodstream.BleedAmount <= 0)
            return;

        var factor = (args.NewPosition.Position - args.OldPosition.Position).Length();
        var normalDamage = ent.Comp.Bleeding * factor;

        _damageable.TryChangeDamage(ent, normalDamage);

        if (ent.Comp.BleedingWorsenAmount is not {} amount)
            return;

        _bloodstream.TryModifyBleedAmount(ent, amount * factor);
    }
}
