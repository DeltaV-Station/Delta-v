using Content.Server.Emp;
using Content.Shared.Damage;
using Content.Shared._DV.Silicons;

namespace Content.Server._DV.Silicons;

public sealed class SiliconEmpSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconEmpComponent, EmpAttemptEvent>(OnEmp);
    }

    private void OnEmp(Entity<SiliconEmpComponent> ent, ref EmpAttemptEvent args)
    {
        args.Cancel(); // Stop all the normal effects of the EMP
        if (args.Damage is not { } damage) return;
        _damageable.TryChangeDamage(ent, damage / 2, false); // Damage is divided by 2 because the event is raised twice (once from entity itself, and another is relayed from it's power cell) and I'm too lazy for an actual fix - NoElka | Make EMP not ignore armor.
    }
}
