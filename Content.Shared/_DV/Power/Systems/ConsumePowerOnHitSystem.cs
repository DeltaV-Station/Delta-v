using Content.Shared._DV.Power.Components;
using Content.Shared.Examine;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._DV.Power.Systems;

public sealed partial class ConsumePowerOnHitSystem : EntitySystem
{
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConsumePowerOnHitComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<ConsumePowerOnHitComponent, ItemToggleActivateAttemptEvent>(OnActivateAttempt);
        SubscribeLocalEvent<ConsumePowerOnHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnExamine(Entity<ConsumePowerOnHitComponent> weapon, ref ExaminedEvent args)
    {
        if (TryComp<BatteryComponent>(weapon.Owner, out var battery))
        {
            var count = _battery.GetRemainingUses((weapon.Owner, battery), weapon.Comp.ChargePerHit);
            args.PushMarkup(Loc.GetString("melee-battery-examine", ("color", "yellow"), ("count", count)));
        }
    }

    private void OnActivateAttempt(Entity<ConsumePowerOnHitComponent> weapon, ref ItemToggleActivateAttemptEvent args)
    {
        if (_battery.GetCharge(weapon.Owner) >= weapon.Comp.ChargePerHit)
            return;

        args.Popup = Loc.GetString("comp-consume-power-insufficient-charge", ("weapon", weapon));
        args.Cancelled = true;
    }

    private void OnMeleeHit(Entity<ConsumePowerOnHitComponent> weapon, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        if (!_battery.TryUseCharge(weapon.Owner, weapon.Comp.ChargePerHit))
        {
            _battery.SetCharge(weapon.Owner, 0);
            _toggle.TryDeactivate(weapon.Owner, args.User);

            var message = Loc.GetString("comp-consume-power-swing-turned-off", ("weapon", weapon));
            _popup.PopupPredicted(message, weapon, args.User, PopupType.MediumCaution);
        }
    }
}
