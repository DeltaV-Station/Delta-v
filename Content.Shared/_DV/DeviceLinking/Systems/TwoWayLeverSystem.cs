using Content.Shared._DV.Psionics.Events;
using Content.Shared.DeviceLinking.Components;

namespace Content.Shared.DeviceLinking.Systems;

public sealed partial class TwoWayLeverSystem
{
    private void OnTelekinesisUsed(Entity<TwoWayLeverComponent> ent, ref TelekinesisUsedEvent args) // Delta-V
    {

        ent.Comp.State = ent.Comp.State switch
        {
            TwoWayLeverState.Middle => ent.Comp.NextSignalLeft ? TwoWayLeverState.Left : TwoWayLeverState.Right,
            TwoWayLeverState.Right => TwoWayLeverState.Middle,
            TwoWayLeverState.Left => TwoWayLeverState.Middle,
            _ => throw new ArgumentOutOfRangeException()
        };

        StateChanged(ent.Owner, ent.Comp);
        args.Cancel();
    }
}
