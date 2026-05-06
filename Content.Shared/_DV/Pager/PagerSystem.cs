using Content.Shared.CartridgeLoader;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.DeviceNetwork.Systems;
using Content.Shared.Fax.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using static Content.Shared._DV.Pager.PagerConstants;

namespace Content.Shared._DV.Pager;

public sealed class PagerSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedDeviceNetworkSystem _deviceNetwork = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PagerComponent, MapInitEvent>(OnMapInit, after: [typeof(SharedDeviceNetworkSystem)]);
        SubscribeLocalEvent<PagerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<PagerComponent, DeviceNetworkPacketEvent>(OnDeviceNetworkPacket);
        SubscribeLocalEvent<PagerComponent, PagerRemoveAddressMessage>(OnRemoveAddress);
        SubscribeLocalEvent<FaxMachineComponent, PageSenderNameEvent>(OnFaxMachineName);
    }

    private void OnMapInit(Entity<PagerComponent> ent, ref MapInitEvent args)
    {
        var payload = new NetworkPayload()
        {
            { DeviceNetworkConstants.Command, PagerConstants.CommandAutoLinkRequest },
        };
        _deviceNetwork.QueuePacket(ent, null, payload);
    }

    private void OnAfterInteract(Entity<PagerComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !TryComp<DeviceNetworkComponent>(args.Target, out var targetNetwork) || !HasComp<PageSenderComponent>(args.Target))
            return;

        var ourNetwork = Comp<DeviceNetworkComponent>(ent);
        if (ourNetwork.DeviceNetId != targetNetwork.DeviceNetId)
        {
            _popup.PopupPredicted(Loc.GetString("pager-error-different-networks", ("sender", args.Target.Value), ("receiver", ent)), args.Target.Value, args.User);
            return;
        }

        // THIS CANNOT BE PREDICTED DUE TO DeviceNetworkComponent BEING UNNETWORKED
        if (!_net.IsServer)
            return;

        if (ent.Comp.Devices.Remove(targetNetwork.Address, out var removedName))
        {
            _popup.PopupEntity(Loc.GetString("pager-connection-removed", ("sender", args.Target.Value), ("receiver", ent)), args.Target.Value);
            Dirty(ent);
            return;
        }

        var evt = new PageSenderNameEvent(Identity.Name(args.Target.Value, EntityManager));
        RaiseLocalEvent(args.Target.Value, ref evt);

        ent.Comp.Devices[targetNetwork.Address] = evt.Name;
        _popup.PopupEntity(Loc.GetString("pager-connection-added", ("sender", args.Target.Value), ("receiver", ent)), args.Target.Value);
        Dirty(ent);
    }

    private void OnRemoveAddress(Entity<PagerComponent> ent, ref PagerRemoveAddressMessage args)
    {
        ent.Comp.Devices.Remove(args.Address);
        Dirty(ent);
    }

    private void OnDeviceNetworkPacket(Entity<PagerComponent> ent, ref DeviceNetworkPacketEvent args)
    {
        if (!args.Data.TryGetValue(DeviceNetworkConstants.Command, out string? command))
        {
            return;
        }

        switch (command)
        {
            case CommandNotify:
            {
                if (!ent.Comp.Devices.TryGetValue(args.SenderAddress, out var name))
                    return;

                if (!args.Data.TryGetValue<string>(DataBody, out var body))
                    return;

                var evt = new CartridgeLoaderNotificationSentEvent(Loc.GetString("pager-notification", ("sender", name)), body);
                RaiseLocalEvent(ent, ref evt);

                return;
            }

            case CommandAutoLink:
            {
                if (!args.Data.TryGetValue<HashSet<ProtoId<DepartmentPrototype>>>(DataDepartments, out var departments))
                    return;

                if (!args.Data.TryGetValue<HashSet<ProtoId<JobPrototype>>>(DataJobs, out var jobs))
                    return;

                if (!(departments.Overlaps(ent.Comp.AutoLinkDepartments) || jobs.Overlaps(ent.Comp.AutoLinkJobs)))
                    return;

                var evt = new PageSenderNameEvent(Identity.Name(args.Sender, EntityManager));
                RaiseLocalEvent(args.Sender, ref evt);
                ent.Comp.Devices[args.SenderAddress] = evt.Name;
                Dirty(ent);

                return;
            }
        }
    }

    private void OnFaxMachineName(Entity<FaxMachineComponent> ent, ref PageSenderNameEvent args)
    {
        args.Name = ent.Comp.FaxName;
    }
}
