using Content.Server.DeviceLinking.Components;
using Content.Shared._DV.Psionics.Events;
using Robust.Shared.Audio;

namespace Content.Server.DeviceLinking.Systems;

public sealed partial class SignalSwitchSystem
{
    private void OnTelekinesisUsed(Entity<SignalSwitchComponent> ent, ref TelekinesisUsedEvent args)
    {
        if (args.Handled)
            return;

        Activate(ent);

        args.Handled = true;
    }

    private void Activate(Entity<SignalSwitchComponent> ent)
    {
        if (_lock.IsLocked(ent.Owner))
            return;

        ent.Comp.State = !ent.Comp.State;
        _deviceLink.InvokePort(ent.Owner, ent.Comp.State ? ent.Comp.OnPort : ent.Comp.OffPort);

        // only send status if it's a toggle switch and not a button
        if (ent.Comp.OnPort != ent.Comp.OffPort)
        {
            _deviceLink.SendSignal(ent.Owner, ent.Comp.StatusPort, ent.Comp.State);
        }

        _audio.PlayPvs(ent.Comp.ClickSound, ent.Owner, AudioParams.Default.WithVariation(0.125f).WithVolume(8f));
    }
}
