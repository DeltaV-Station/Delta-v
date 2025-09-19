using Content.Shared.Actions;

namespace Content.Shared._DV.Movement;

public abstract class SharedCursorOffsetActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CursorOffsetActionComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CursorOffsetActionComponent, CursorOffsetActionEvent>(OnAction);
    }

    private void OnInit(Entity<CursorOffsetActionComponent> ent, ref ComponentInit args)
    {
        _actions.AddAction(ent, ref ent.Comp.CursorOffsetActionEntity, ent.Comp.CursorOffsetActionId );

        if (_actions.GetAction(ent.Comp.CursorOffsetActionEntity) is not { Comp.UseDelay: not null })
        {
            _actions.StartUseDelay(ent.Comp.CursorOffsetActionEntity);
        }
    }

    protected virtual void OnAction(Entity<CursorOffsetActionComponent> ent, ref CursorOffsetActionEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.Active = !ent.Comp.Active;
        Dirty(ent);

        args.Handled = true;
    }
}
