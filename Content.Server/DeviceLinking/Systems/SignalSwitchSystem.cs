using Content.Server.DeviceLinking.Components;
using Content.Server.DeviceNetwork;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.DeviceLinking.Systems;

public sealed class SignalSwitchSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly LockSystem _lock = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SignalSwitchComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SignalSwitchComponent, ActivateInWorldEvent>(OnActivated);

        SubscribeLocalEvent<SignalSwitchComponent, TelekinesisUsedEvent>(OnTelekinesisUsed); // Delta-V
    }

    private void OnInit(EntityUid uid, SignalSwitchComponent comp, ComponentInit args)
    {
        _deviceLink.EnsureSourcePorts(uid, comp.OnPort, comp.OffPort, comp.StatusPort);
    }

    private void OnActivated(EntityUid uid, SignalSwitchComponent comp, ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        Activate((uid, comp));

        args.Handled = true;
    }

    private void OnTelekinesisUsed(Entity<SignalSwitchComponent> ent, ref TelekinesisUsedEvent args) // Delta-V
    {
        if (args.Cancelled)
            return;

        Activate(ent);

        args.Cancel();
    }

    private void Activate(Entity<SignalSwitchComponent> ent) // Delta-V, isolated this from OnActivated
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
