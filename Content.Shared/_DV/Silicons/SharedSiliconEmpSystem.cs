using Content.Shared.Emp;
using Content.Shared.Damage;
using Content.Shared._DV.Silicons.Components;

namespace Content.Shared._DV.Silicons;

public abstract class SharedSiliconEmpSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconEmpComponent, EmpPulseEvent>(OnEmpPulse);
    }

    private void OnEmpPulse(Entity<SiliconEmpComponent> ent, ref EmpPulseEvent args)
    {
        if (args.Damage is not { } damage) return;
        _damageable.TryChangeDamage(ent, damage / 2, false); // Damage is divided by 2 because the event is raised twice (once from entity itself, and another is relayed from it's power cell) and I'm too lazy for an actual fix - NoElka | Make EMP not ignore armor.
    }
}
