using Content.Server.Emp;
using Content.Shared._DV.Abilities;
using Content.Shared.Actions;

namespace Content.Server._DV.Abilities;

public sealed partial class TechnokineticPulseAbilitySystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EmpSystem _emp = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TechnokineticPulseAbilityComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TechnokineticPulseAbilityComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TechnokineticPulseAbilityComponent, TechnokineticPulseActionEvent>(OnTechnokineticPulseAction);
    }

    private void OnMapInit(Entity<TechnokineticPulseAbilityComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.TechnokineticPulseActionEntity, ent.Comp.TechnokineticPulseActionId);
        _actions.StartUseDelay(ent.Comp.TechnokineticPulseActionEntity);
    }

    private void OnShutdown(Entity<TechnokineticPulseAbilityComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity.Owner, entity.Comp.TechnokineticPulseActionEntity);
    }

    private void OnTechnokineticPulseAction(Entity<TechnokineticPulseAbilityComponent> entity, ref TechnokineticPulseActionEvent args)
    {
        if (args.Handled)
            return;

        _emp.EmpPulse(_transform.GetMapCoordinates(entity), entity.Comp.Range, entity.Comp.EnergyConsumption, entity.Comp.DisableDuration);

        args.Handled = true;
    }
}
