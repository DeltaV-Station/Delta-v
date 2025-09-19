using System.Numerics;
using Content.Client.Movement.Components;
using Content.Shared._DV.Movement;
using Robust.Client.Timing;

namespace Content.Client._DV.Movement;

public sealed class CursorOffsetActionSystem : SharedCursorOffsetActionSystem
{
    [Dependency] private readonly IClientGameTiming _gameTiming = default!;

    protected override void OnAction(Entity<CursorOffsetActionComponent> ent, ref CursorOffsetActionEvent args)
    {
        base.OnAction(ent, ref args);

        if (!TryComp<EyeCursorOffsetComponent>(ent, out var eyeOffset))
            return;

        eyeOffset.Enabled = ent.Comp.Active;

        if (_gameTiming.IsFirstTimePredicted)
            eyeOffset.CurrentPosition = Vector2.Zero;
    }
}
