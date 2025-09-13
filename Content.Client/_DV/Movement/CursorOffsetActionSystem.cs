using System.Diagnostics;
using System.Numerics;
using Content.Client.Movement.Components;
using Content.Client.Movement.Systems;
using Content.Shared._DV.Movement;
using Content.Shared.Camera;
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

    public void OnAction(Entity<CursorOffsetActionComponent> ent, ref CursorOffsetActionEvent args)
    {
        if (!TryComp(ent.Owner, out EyeCursorOffsetComponent? cursorOffsetComp))
            return;

        if (!ent.Comp.Active && _gameTiming.IsFirstTimePredicted)
            cursorOffsetComp.CurrentPosition = Vector2.Zero;
    }

    private void OnGetEyeOffset(Entity<CursorOffsetActionComponent> ent, ref GetEyeOffsetEvent args)
    {
        if (!ent.Comp.Active)
        {
            Log.Info("action not active, setting to zero");
            args.Offset = Vector2.Zero;
            return;
        }

        var offset = _eyeOffset.OffsetAfterMouse(ent.Owner, null);
        if (offset == null)
        {
            Log.Info("offset was null, skipping");
            return;
        }

        args.Offset += offset.Value; //?????
    }
}
