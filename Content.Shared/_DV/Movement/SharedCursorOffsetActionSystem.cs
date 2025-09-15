using Content.Shared.Actions;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared._DV.Movement;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedCursorOffsetActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedContentEyeSystem _eye = default!;

    /// <inheritdoc/>
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

        Log.Debug("shared active is now " + ent.Comp.Active);

        /*if(ent.Comp.Active)
        {
            AddComp<EyeCursorOffsetComponent>(ent.Owner);
            Log.Debug("we ADDING this component for REAL!");
        }
        else
        {
            RemComp<EyeCursorOffsetComponent>(ent.Owner);
            Log.Debug("we REMOVING this component for REAL!");
        }*/

        Dirty(ent);

        args.Handled = true;
    }
}
