using System.Diagnostics;
using System.Numerics;
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
        SubscribeLocalEvent<CursorOffsetActionComponent, GetEyeOffsetEvent>(OnGetEyeOffset);
    }

    protected override void OnAction(Entity<CursorOffsetActionComponent> ent, ref CursorOffsetActionEvent args)
    {
        base.OnAction(ent, ref args);

        if (!TryComp(ent.Owner, out EyeCursorOffsetComponent? cursorOffsetComp))
            return;

        Log.Info("im gonna go fucking insane");

        if (_gameTiming.IsFirstTimePredicted)
        {
            Log.Info("goidapredict");
            cursorOffsetComp.CurrentPosition = Vector2.Zero;
        }
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
}
