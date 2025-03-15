using System.Diagnostics.CodeAnalysis; // DeltaV
using Content.Client._DV.Power.Components; // DeltaV
using Content.Client.Popups; // DeltaV
using Content.Shared._DV.Power.Components; // DeltaV
using Content.Shared.Containers.ItemSlots; // DeltaV
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client.PowerCell;

[UsedImplicitly]
public sealed class PowerCellSystem : SharedPowerCellSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly PopupSystem _popup = default!; // DeltaV
    [Dependency] private readonly ItemSlotsSystem _slot = default!; // DeltaV

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PowerCellVisualsComponent, AppearanceChangeEvent>(OnPowerCellVisualsChange);
    }

    /// <inheritdoc/>
    public override bool HasActivatableCharge(EntityUid uid, PowerCellDrawComponent? battery = null, PowerCellSlotComponent? cell = null,
        EntityUid? user = null)
    {
        if (!Resolve(uid, ref battery, ref cell, false))
            return true;

        return battery.CanUse;
    }

    /// <inheritdoc/>
    public override bool HasDrawCharge(
        EntityUid uid,
        PowerCellDrawComponent? battery = null,
        PowerCellSlotComponent? cell = null,
        EntityUid? user = null)
    {
        if (!Resolve(uid, ref battery, ref cell, false))
            return true;

        return battery.CanDraw;
    }

    // Begin DeltaV changes
    public override bool HasCharge(EntityUid uid, float charge, PowerCellSlotComponent? component = null, EntityUid? user = null)
    {
        if (!TryGetBatteryFromSlot(uid, out _, out var battery, component))
        {
            if (user != null)
                _popup.PopupClient(Loc.GetString("power-cell-no-battery"), uid, user.Value);

            return false;
        }

        if (battery.CurrentCharge < charge)
        {
            if (user != null)
                _popup.PopupClient(Loc.GetString("power-cell-insufficient"), uid, user.Value);

            return false;
        }

        return true;
    }

    public override bool TryGetBatteryFromSlot(EntityUid uid,
        [NotNullWhen(true)] out EntityUid? batteryEnt,
        [NotNullWhen(true)] out SharedBatteryComponent? sharedBattery,
        PowerCellSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
        {
            batteryEnt = null;
            sharedBattery = null;
            return false;
        }

        if (_slot.TryGetSlot(uid, component.CellSlotId, out ItemSlot? slot))
        {
            batteryEnt = slot.Item;
            var hasComp = TryComp<BatteryComponent>(slot.Item, out var battery);
            sharedBattery = battery;
            return hasComp;
        }

        batteryEnt = null;
        sharedBattery = null;
        return false;
    }
    // End DeltaV changes

    private void OnPowerCellVisualsChange(EntityUid uid, PowerCellVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!args.Sprite.TryGetLayer((int) PowerCellVisualLayers.Unshaded, out var unshadedLayer))
            return;

        if (_appearance.TryGetData<byte>(uid, PowerCellVisuals.ChargeLevel, out var level, args.Component))
        {
            if (level == 0)
            {
                unshadedLayer.Visible = false;
                return;
            }

            unshadedLayer.Visible = true;
            args.Sprite.LayerSetState(PowerCellVisualLayers.Unshaded, $"o{level}");
        }
    }

    private enum PowerCellVisualLayers : byte
    {
        Base,
        Unshaded,
    }
}
