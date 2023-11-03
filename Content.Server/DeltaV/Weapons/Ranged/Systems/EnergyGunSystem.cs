using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Toggleable;
using Content.Server.DeltaV.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.DeltaV.Weapons.Ranged.Systems;

public sealed class EnergyGunSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnergyGunComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<EnergyGunComponent, ExaminedEvent>(OnExamined);
    }

    private void OnUseInHand(EntityUid uid, EnergyGunComponent comp, UseInHandEvent args)
    {
        if (comp.Activated)
        {
            TurnDisable(uid, comp, args.User);
        }
        else
        {
            TurnLethal(uid, comp, args.User);
        }
    }

    private void OnExamined(EntityUid uid, EnergyGunComponent comp, ExaminedEvent args)
    {
        var msg = comp.Activated
            ? Loc.GetString("comp-energygun-examined-lethal")
            : Loc.GetString("comp-energygun-examined-disable");
        args.PushMarkup(msg);
    }

    private void TurnDisable(EntityUid uid, EnergyGunComponent comp, EntityUid user)
    {
        if (!comp.Activated)
            return;

        if (TryComp<AppearanceComponent>(uid, out var appearance) &&
            TryComp<ItemComponent>(uid, out var item))
        {
            _item.SetHeldPrefix(uid, "disable", item);
            _appearance.SetData(uid, ToggleVisuals.Toggled, false, appearance);
        }

        _popup.PopupEntity(Loc.GetString("energygun-mode-disable"), user, user);

        SetFireMode(uid, comp, comp.DisableMode);

        comp.Activated = false;
        Dirty(uid, comp);
    }

    private void TurnLethal(EntityUid uid, EnergyGunComponent comp, EntityUid user)
    {
        if (comp.Activated)
            return;

        if (EntityManager.TryGetComponent<AppearanceComponent>(uid, out var appearance) &&
            EntityManager.TryGetComponent<ItemComponent>(uid, out var item))
        {
            _item.SetHeldPrefix(uid, "lethal", item);
            _appearance.SetData(uid, ToggleVisuals.Toggled, true, appearance);
        }

        _popup.PopupEntity(Loc.GetString("energygun-mode-lethal"), user, user);

        SetFireMode(uid, comp, comp.LethalMode);

        comp.Activated = true;
        Dirty(uid, comp);
    }

    private void SetFireMode(EntityUid uid, EnergyGunComponent comp, EnergyWeaponFireMode fireMode)
    {
        if (fireMode?.Prototype == null)
            return;

        if (TryComp(uid, out ProjectileBatteryAmmoProviderComponent? projectileBatteryAmmoProvider))
        {
            if (!_prototypeManager.TryIndex<EntityPrototype>(fireMode.Prototype, out var _))
                return;

            projectileBatteryAmmoProvider.Prototype = fireMode.Prototype;
            projectileBatteryAmmoProvider.FireCost = fireMode.FireCost;
        }
    }
}
