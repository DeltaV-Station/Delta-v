using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Emp;

namespace Content.Shared._Shitmed.Cybernetics;

/// <summary>
/// Prevents enabling body parts/organs that are disabled by an EMP.
/// Does nothing on its own, server system has to allow EMP to work with <see cref="CyberneticsComponent"/>.
/// </summary>
public abstract class SharedCyberneticsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmpDisabledComponent, MechanismEnableAttemptEvent>(OnEnableAttempt);
    }

    private void OnEnableAttempt(Entity<EmpDisabledComponent> ent, ref MechanismEnableAttemptEvent args)
    {
        args.Cancelled = true;
    }
}
