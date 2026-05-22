using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.DeviceNetwork.Systems;

namespace Content.Shared._DV.Pager;

public sealed class PageSenderSystem : EntitySystem
{
    [Dependency] private readonly SharedDeviceNetworkSystem _deviceNetwork = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PageSenderComponent, MapInitEvent>(OnMapInit, after: [typeof(SharedDeviceNetworkSystem)]);
        SubscribeLocalEvent<PageSenderComponent, DeviceNetworkPacketEvent>(OnDeviceNetworkPacket);
    }

    private void OnMapInit(Entity<PageSenderComponent> ent, ref MapInitEvent args)
    {
        AutoLink(ent, null);
    }

    private void OnDeviceNetworkPacket(Entity<PageSenderComponent> ent, ref DeviceNetworkPacketEvent args)
    {
        if (!args.Data.TryGetValue(DeviceNetworkConstants.Command, out string? command) ||
            command != PagerConstants.CommandAutoLinkRequest)
        {
            return;
        }

        AutoLink(ent, args.SenderAddress);
    }

    private void AutoLink(Entity<PageSenderComponent> ent, string? address)
    {
        var payload = new NetworkPayload()
        {
            { DeviceNetworkConstants.Command, PagerConstants.CommandAutoLink },
            { PagerConstants.DataDepartments, ent.Comp.AutoLinkDepartments },
            { PagerConstants.DataJobs, ent.Comp.AutoLinkJobs },
        };
        _deviceNetwork.QueuePacket(ent, address, payload);
    }

    public void Notify(Entity<PageSenderComponent?> ent, string body)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        var payload = new NetworkPayload()
        {
            { DeviceNetworkConstants.Command, PagerConstants.CommandNotify },
            { PagerConstants.DataBody, body },
        };
        _deviceNetwork.QueuePacket(ent, null, payload);
    }
}
