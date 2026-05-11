using Content.Server.Electrocution;
using Content.Server.Lightning;
using Content.Server.Power.EntitySystems;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;

namespace Content.Server._DV.Psionics.Systems.PsionicPowers;

public sealed class NoosphericZapPowerSystem : SharedNoosphericZapPowerSystem
{
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerCellSlotComponent, NoosphericallyZappedEvent>(OnBatterySlotZapped);
        SubscribeLocalEvent<BatteryComponent, NoosphericallyZappedEvent>(OnBatteryZapped);
    }

    protected override void OnPowerUsed(Entity<NoosphericZapPowerComponent> psionic, ref NoosphericZapPowerActionEvent args)
    {
        // As this can target batteries, it doesn't require the target to be psionic.
        if (!Psionic.CanBeTargeted(args.Target, ignorePsionicRequirement: true, hasAggressor: args.Performer))
            return;

        var ev = new NoosphericallyZappedEvent(psionic.Comp.AddedBatteryCharge, args.Performer);
        RaiseLocalEvent(args.Target, ref ev);

        // If they don't have anything else, we check if they're a potential psionic.
        if (!ev.CanZap && !Psionic.CanBeTargeted(args.Target, hasAggressor: args.Performer))
            return;

        var message = Loc.GetString("psionic-power-noospheric-zap-user", ("user", Identity.Entity(args.Performer, EntityManager)));
        Popup.PopupEntity(message, args.Performer, PopupType.LargeCaution);

        _lightning.ShootLightning(args.Performer, args.Target, psionic.Comp.LightningPrototpyeId);
        _electrocution.TryDoElectrocution(args.Target, args.Performer, psionic.Comp.ShockDamage, psionic.Comp.StunDuration, true);

        AfterPowerUsed(psionic, args.Performer);
    }

    private void OnBatterySlotZapped(Entity<PowerCellSlotComponent> batterySlot, ref NoosphericallyZappedEvent args)
    {
        if (!_powerCell.TryGetBatteryFromEntityOrSlot(batterySlot.Owner, out var battery))
            return;

        ChargeBattery(battery.Value.AsNullable(), args.RechargeAmount, batterySlot);
        args.CanZap = true;
    }

    private void OnBatteryZapped(Entity<BatteryComponent> battery, ref NoosphericallyZappedEvent args)
    {
        ChargeBattery(battery.AsNullable(), args.RechargeAmount, battery);
        args.CanZap = true;
    }

    private void ChargeBattery(Entity<BatteryComponent?> battery, float amount, EntityUid container)
    {
        var message = Loc.GetString("psionic-power-noospheric-zap-battery", ("battery", Identity.Entity(container, EntityManager)));
        Popup.PopupEntity(message, battery, PopupType.Medium);
        _battery.ChangeCharge(battery, amount);
    }
}
