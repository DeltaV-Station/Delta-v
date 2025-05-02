using Content.Server.Power.Components; // ough BatteryComponent why are you in server
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Shared._DV.Augments;
using Content.Shared.Alert;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.PowerCell.Components;

namespace Content.Server._DV.Augments;

public sealed class AugmentPowerCellSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly AugmentSystem _augment = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HasAugmentPowerCellSlotComponent, SearchForBatteryEvent>(OnSearchForBattery);

        SubscribeLocalEvent<AugmentPowerCellSlotComponent, ChargeChangedEvent>(OnChargeChanged);
        SubscribeLocalEvent<AugmentPowerCellSlotComponent, PowerCellChangedEvent>(OnPowerCellChanged);
    }

    private void OnSearchForBattery(Entity<HasAugmentPowerCellSlotComponent> ent, ref SearchForBatteryEvent args)
    {
        // or by checking for an augment power cell
        if (TryGetAugmentPowerCell(ent) is (_, var insertedBattery) && insertedBattery is {} battery)
        {
            args.Uid = battery.Owner;
            args.Component = battery.Comp;
            args.Handled = true;
            return;
        }
    }

    private void OnChargeChanged(Entity<AugmentPowerCellSlotComponent> ent, ref ChargeChangedEvent args)
    {
        CheckCharge(ent);
    }

    private void OnPowerCellChanged(Entity<AugmentPowerCellSlotComponent> ent, ref PowerCellChangedEvent args)
    {
        CheckCharge(ent);
    }

    private void CheckCharge(Entity<AugmentPowerCellSlotComponent> ent)
    {
        var hasCharge = _powerCell.HasDrawCharge(ent);
        if (hasCharge == ent.Comp.HasCharge)
            return;

        if (CompOrNull<OrganComponent>(ent)?.Body is not {} body)
            return;

        ent.Comp.HasCharge = hasCharge;
        Dirty(ent);
        if (!hasCharge)
        {
            _popup.PopupEntity(Loc.GetString("augments-power-cell-emptied"), body, body, PopupType.MediumCaution);
            // not raising event here since PowerCellDraw update should do it
            return;
        }

        // inform augments they now have some power available
        var ev = new AugmentPowerAvailableEvent(body);
        _augment.RelayEvent(body, ref ev);
    }

    public (Entity<AugmentPowerCellSlotComponent, OrganComponent, PowerCellSlotComponent> Organ, Entity<BatteryComponent>? Battery)? TryGetAugmentPowerCell(EntityUid body)
    {
        foreach (var organ in _body.GetBodyOrganEntityComps<AugmentPowerCellSlotComponent>(body))
        {
            if (!TryComp<PowerCellSlotComponent>(organ, out var powerCellSlot))
                continue;

            var entity = new Entity<AugmentPowerCellSlotComponent, OrganComponent, PowerCellSlotComponent>(organ.Owner, organ.Comp1, organ.Comp2, powerCellSlot);

            if (_powerCell.TryGetBatteryFromSlot(organ, out var batteryUid, out var batteryComp))
            {
                return (entity, new(batteryUid.Value, batteryComp));
            }
            return (entity, null);
        }
        return null;
    }

    public (Entity<AugmentPowerCellSlotComponent, OrganComponent, PowerCellSlotComponent> Organ, Entity<BatteryComponent>? Battery)? TryGetAugmentPowerCellFromAugment(EntityUid augment)
    {
        if (CompOrNull<OrganComponent>(augment)?.Body is not {} body)
            return null;

        return TryGetAugmentPowerCell(body);
    }

    public bool TryDrawPower(EntityUid augment, float amount)
    {
        // need it for popups so getting it explicitly instead of using above method
        if (CompOrNull<OrganComponent>(augment)?.Body is not {} body)
            return false;

        if (TryGetAugmentPowerCell(body) is not (_, var battery))
        {
            _popup.PopupEntity(Loc.GetString("augments-no-power-cell-slot"), body, body);
            return false;
        }

        if (battery is not {} insertedBattery)
        {
            _popup.PopupEntity(Loc.GetString("power-cell-no-battery"), body, body);
            return false;
        }

        if (!_battery.TryUseCharge(insertedBattery.Owner, amount))
        {
            _popup.PopupEntity(Loc.GetString("power-cell-insufficient"), body, body);
            return false;
        }

        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HasAugmentPowerCellSlotComponent>();
        while (query.MoveNext(out var owner, out _))
        {
            if (_mobState.IsDead(owner))
                continue;

            var powerCell = TryGetAugmentPowerCell(owner);
            if (powerCell is not {} power)
                continue;
            var augment = power.Organ;

            if (power.Battery is not {} insertedBattery)
            {
                if (_alerts.IsShowingAlert(owner, augment.Comp1.BatteryAlert))
                {
                    _alerts.ClearAlert(owner, augment.Comp1.BatteryAlert);
                    _alerts.ShowAlert(owner, augment.Comp1.NoBatteryAlert);
                }
                continue;
            }

            if (_alerts.IsShowingAlert(owner, augment.Comp1.NoBatteryAlert))
            {
                _alerts.ClearAlert(owner, augment.Comp1.NoBatteryAlert);
            }

            var chargePercent = (short) MathF.Round(insertedBattery.Comp.CurrentCharge / insertedBattery.Comp.MaxCharge * 10f);
            _alerts.ShowAlert(owner, augment.Comp1.BatteryAlert, chargePercent);
        }
    }
}
