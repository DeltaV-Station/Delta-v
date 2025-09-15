using System.Diagnostics;
using System.Numerics;
using Content.Client.Movement.Components;
using Content.Client.Movement.Systems;
using Content.Shared._DV.Movement;
using Content.Shared.Camera;
using Content.Shared.Movement.Components;
using Robust.Client.Timing;

namespace Content.Client._DV.Movement;

public sealed class CursorOffsetActionSystem : SharedCursorOffsetActionSystem
{
    [Dependency] private readonly EyeCursorOffsetSystem _eyeOffset = default!;
    [Dependency] private readonly IClientGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        //SubscribeLocalEvent<CursorOffsetActionComponent, CursorOffsetActionEvent>(OnAction);
        //SubscribeLocalEvent<CursorOffsetActionComponent, GetEyeOffsetEvent>(OnGetEyeOffset);
    }

    protected override void OnAction(Entity<CursorOffsetActionComponent> ent, ref CursorOffsetActionEvent args)
    {
        base.OnAction(ent, ref args);

        Log.Info("no OnAction client code (awesome)");
    }

    private void OnGetEyeOffset(Entity<CursorOffsetActionComponent> ent, ref GetEyeOffsetEvent args)
    {
        if (!ent.Comp.Active)
            return;

        var offset = _eyeOffset.OffsetAfterMouse(ent.Owner, null);
        if (offset == null)
            return;

        Log.Info("Current offset " + offset.Value);

        args.Offset += offset.Value;
    }

    public override void AddOrRemoveEyeOffset(Entity<CursorOffsetActionComponent> ent, bool add)
    {
        if (add)
        {
            AddComp<EyeCursorOffsetComponent>(ent);
        }
        else
        {
            RemComp<EyeCursorOffsetComponent>(ent);
        }
    }
}
