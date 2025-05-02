using Content.Server.Emp;
using Content.Shared.Body.Systems;
using Content.Shared._Shitmed.Cybernetics;

namespace Content.Server._Shitmed.Cybernetics;

public sealed class CyberneticsSystem : SharedCyberneticsSystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CyberneticsComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<CyberneticsComponent, EmpDisabledRemoved>(OnEmpDisabledRemoved);
    }

    private void OnEmpPulse(Entity<CyberneticsComponent> cyberEnt, ref EmpPulseEvent ev)
    {
        ev.Affected = true;
        ev.Disabled = true;

        _body.TryDisableMechanism(cyberEnt.Owner);
    }

    private void OnEmpDisabledRemoved(Entity<CyberneticsComponent> cyberEnt, ref EmpDisabledRemoved ev)
    {
        _body.TryEnableMechanism(cyberEnt.Owner);
    }
}
