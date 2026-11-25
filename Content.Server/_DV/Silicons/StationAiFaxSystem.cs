using Content.Server.Fax;
using Content.Shared._DV.Silicons;
using Content.Shared.Fax;
using Content.Shared.Fax.Components;

namespace Content.Server._DV.Silicons;

public sealed class StationAiFaxSystem : SharedStationAiFaxSystem
{
    [Dependency] private readonly FaxSystem _fax = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<StationAiFaxComponent>(FaxUiKey.Key,
            subs =>
            {
                subs.Event<StationAiFaxSendMessage>(OnSend);
                subs.Event<StationAiFaxDuplicateMessage>(OnDuplicate);
            });
    }

    private void OnSend(Entity<StationAiFaxComponent> ent, ref StationAiFaxSendMessage args)
    {
        if (GetDocument(ent, args.Document) is not { } document)
            return;

        _fax.Send(ent, Comp<FaxMachineComponent>(ent), args, document);
    }

    private void OnDuplicate(Entity<StationAiFaxComponent> ent, ref StationAiFaxDuplicateMessage args)
    {
        if (GetDocument(ent, args.Document) is not { } document)
            return;

        _fax.Copy(ent, Comp<FaxMachineComponent>(ent), args, document);
    }
}
