using Content.Shared.DeltaV.Salvage.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.Salvage.Systems;

public sealed class MiningVoucherSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MiningVoucherComponent, AfterInteractEvent>(OnAfterInteract);
        Subs.BuiEvents<MiningVoucherComponent>(MiningVoucherUiKey.Key, subs =>
        {
            subs.Event<MiningVoucherSelectMessage>(OnSelect);
        });
    }

    private void OnAfterInteract(Entity<MiningVoucherComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target is not {} target)
            return;

        if (_whitelist.IsWhitelistFail(ent.Comp.VendorWhitelist, target))
            return;

        var user = args.User;
        args.Handled = true;

        if (ent.Comp.Selected is not {} index)
        {
            _popup.PopupClient(Loc.GetString("mining-voucher-select-first"), target, user);
            return;
        }

        if (!_power.IsPowered(target))
        {
            _popup.PopupClient(Loc.GetString("mining-voucher-vendor-unpowered", ("vendor", target)), target, user);
            return;
        }

        _audio.PlayPvs(ent.Comp.RedeemSound, target);
        Redeem(ent, index, user);
    }

    private void OnSelect(Entity<MiningVoucherComponent> ent, ref MiningVoucherSelectMessage args)
    {
        var index = args.Index;
        if (index < 0 || index >= ent.Comp.Kits.Count)
            return;

        var user = args.Actor;
        var kit = _proto.Index(ent.Comp.Kits[index]);
        var name = Loc.GetString(kit.Name);
        _popup.PopupEntity(Loc.GetString("mining-voucher-selected", ("kit", name)), user, user);

        ent.Comp.Selected = index;
        Dirty(ent);
    }

    public void Redeem(Entity<MiningVoucherComponent> ent, int index, EntityUid user)
    {
        var kit = _proto.Index(ent.Comp.Kits[index]);
        var xform = Transform(ent);
        foreach (var id in kit.Content)
        {
            SpawnNextToOrDrop(id, ent, xform);
        }

        QueueDel(ent);
    }
}
