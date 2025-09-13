using Content.Shared._DV.Movement;
using Content.Shared.Actions;
using Content.Shared.Movement.Systems;

namespace Content.Server._DV.Movement;

public sealed class CursorOffsetActionSystem : SharedCursorOffsetActionSystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedContentEyeSystem _eye = default!;

    public override void Initialize()
    {
        base.Initialize();

        //SubscribeLocalEvent<CursorOffsetActionComponent, ComponentInit>(OnInit);
        //SubscribeLocalEvent<CursorOffsetActionComponent, CursorOffsetActionEvent>(OnAction);
    }

    private void OnInit(Entity<CursorOffsetActionComponent> ent, ref ComponentInit args)
    {
        _actions.AddAction(ent, ref ent.Comp.CursorOffsetActionEntity, ent.Comp.CursorOffsetActionId );

        if (_actions.GetAction(ent.Comp.CursorOffsetActionEntity) is not { Comp.UseDelay: not null })
        {
            _actions.StartUseDelay(ent.Comp.CursorOffsetActionEntity);
        }
    }

    protected override void OnAction(Entity<CursorOffsetActionComponent> ent, ref CursorOffsetActionEvent args)
    {
        base.OnAction(ent, ref args);

        Log.Info("okay running the server code now trust");

        _eye.UpdatePvsScale(args.Performer);
    }
}
