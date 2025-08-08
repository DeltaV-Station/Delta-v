using Content.Server.Actions;
using Content.Shared._DV.Light;


namespace Content.Server._DV.Light;

public sealed partial class ToggleLightActionSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ToggleLightActionComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<ToggleLightActionComponent> entity, ref MapInitEvent args)
    {
        _actions.AddAction(entity, ref entity.Comp.ToggleLightingActionEntity, entity.Comp.ToggleLightingAction);
    }
}
