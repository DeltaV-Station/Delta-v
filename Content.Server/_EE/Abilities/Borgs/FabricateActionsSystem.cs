using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Borgs;

public sealed partial class FabricateActionsSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FabricateActionsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FabricateActionsComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FabricateActionsComponent, FabricateActionEvent>(OnFabricate);
    }


    private void OnStartup(Entity<FabricateActionsComponent> entity, ref ComponentStartup args)
    {
        foreach (var actionId in entity.Comp.Actions)
        {
            EntityUid? actionEntity = null;
            if (_actions.AddAction(entity, ref actionEntity, actionId))
                entity.Comp.ActionEntities[actionId] = actionEntity.Value;
        }
    }

    private void OnShutdown(Entity<FabricateActionsComponent> entity, ref ComponentShutdown args)
    {
        foreach (var (actionId, actionEntity) in entity.Comp.ActionEntities)
        {
            if (actionEntity is not { Valid: true })
                continue;

            _actions.RemoveAction(entity, actionEntity);
            entity.Comp.ActionEntities.Remove(actionId);
        }
    }

    private void OnFabricate(Entity<FabricateActionsComponent> entity, ref FabricateActionEvent args)
    {
        if (args.Handled || !_actionBlocker.CanConsciouslyPerformAction(entity))
            return;

        SpawnNextToOrDrop(args.Fabrication, entity);
        args.Handled = true;
    }
}
