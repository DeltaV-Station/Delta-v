using Content.Shared.Emp;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components;
using Content.Shared._DV.Silicons.Components;

namespace Content.Shared._DV.Silicons;

public abstract class SharedSiliconEmpSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    /// <summary>
    ///     If the entity has the <see cref="SiliconEmpComponent"/>, then it should take damage instead of draining its battery.
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <returns>true if the entity has <see cref="SiliconEmpComponent"/></returns>
    public bool ShouldTakeDamageInsteadOfPowerDrain(Entity<SiliconEmpComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        return true;
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconEmpComponent, EmpPulseEvent>(OnEmpPulse);
    }

    private void OnEmpPulse(Entity<SiliconEmpComponent> ent, ref EmpPulseEvent args)
    {
        if (args.Damage is not { } damage)
            return;

        _damageable.TryChangeDamage(ent.Owner, damage, false);
    }
}
