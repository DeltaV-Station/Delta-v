
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Shared._DV.Augments;
using Content.Shared.Alert;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Power.Components; // ough BatteryComponent why are you in server
using Content.Shared.PowerCell.Components;

namespace Content.Server._DV.Augments;

public sealed class AugmentPowerCellSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HasAugmentPowerCellSlotComponent, SearchForBatteryEvent>(OnSearchForBattery);
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
        if (!TryComp<OrganComponent>(augment, out var organ) || organ.Body is not {} uid)
            return null;

        return TryGetAugmentPowerCell(uid);
    }

    public bool TryDrawPower(EntityUid augment, float amount)
    {
        if (!TryComp<OrganComponent>(augment, out var organ) || organ.Body is not {} body)
            return false;

        if (TryGetAugmentPowerCellFromAugment(augment) is not (_, var battery))
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

        var query = EntityQueryEnumerator<HasAugmentPowerCellSlotComponent, MobStateComponent>();
        while (query.MoveNext(out var owner, out _, out var mobState))
        {
            if (_mobState.IsDead(owner, mobState))
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
