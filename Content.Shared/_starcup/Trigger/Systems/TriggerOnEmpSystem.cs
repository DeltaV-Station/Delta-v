using Content.Shared._starcup.Trigger.Components;
using Content.Shared.Emp;
using Content.Shared.Trigger;

namespace Content.Shared._starcup.Trigger.Systems;

public sealed partial class TriggerOnEmpSystem : TriggerOnXSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriggerOnEmpComponent, EmpPulseEvent>(OnEmpPulse);
    }

    private void OnEmpPulse(Entity<TriggerOnEmpComponent> ent, ref EmpPulseEvent args)
    {
        Trigger.Trigger(ent.Owner, args.User, ent.Comp.KeyOut);
    }
}
