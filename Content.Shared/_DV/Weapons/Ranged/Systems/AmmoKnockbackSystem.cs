using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._DV.Weapons.Ranged.Systems;

public sealed class AmmoKnockbackSystem : EntitySystem
{
    [Dependency] private SharedStunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AmmoKnockbackComponent, ShooterImpulseEvent>(OnImpulse);
        SubscribeLocalEvent<AmmoKnockbackComponent, ImpulseAppliedEvent>(OnImpulseApplied);
    }

    private void OnImpulse(Entity<AmmoKnockbackComponent> ent, ref ShooterImpulseEvent args)
    {
        args.Push = true;
        if (args.Gun.Holder is not { })
        {
            args.ImpulseStrength = ent.Comp.KnockbackStrength / 10;
            return;
        }
        args.ImpulseStrength = ent.Comp.KnockbackStrength;
    }

    private void OnImpulseApplied(Entity<AmmoKnockbackComponent> ent, ref ImpulseAppliedEvent args)
    {
        if (ent.Comp.Knockdown)
        {
            _stunSystem.TryUpdateParalyzeDuration(args.User, TimeSpan.FromSeconds(ent.Comp.KnockdownTime));
        }
    }
}
