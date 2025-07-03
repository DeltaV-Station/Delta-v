using System.Diagnostics.CodeAnalysis;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Shared._DV.Battery.Components;
using Content.Shared._DV.Battery.EntitySystems;
using Content.Shared.Alert;
using Content.Shared.Clothing;
using Content.Shared.PowerCell.Components;
using Content.Shared.Rounding;
using Robust.Shared.Containers;

namespace Content.Server._DV.Battery.EntitySystems;

/// <summary>
/// Server side handling for battery providers.
/// </summary>
public sealed partial class BatteryProviderSystem : SharedBatteryProviderSystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly BatterySystem _batterySystem = default!;
    [Dependency] private readonly PowerCellSystem _powerCellSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryProviderComponent, ContainerIsInsertingAttemptEvent>(OnProviderInsertCellAttempt);
        SubscribeLocalEvent<BatteryProviderComponent, ContainerIsRemovingAttemptEvent>(OnProviderRemoveCellAttempt);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<BatteryProviderComponent>();
        while (query.MoveNext(out var uid, out var suit))
        {
            SetBatteryPowerAlert((uid, suit));
        }
    }

    /// <summary>
    /// Attempts to use charge from a battery provided by the specified entity.
    /// </summary>
    /// <param name="provider">The entity which may have a battery provider attached.</param>
    /// <param name="equipment">The equipment the charge is for, used as validation.</param>
    /// <param name="charge">The amount of charge to use.</param>
    /// <returns>True if the charge was used and the equipment was valid, false otherwise.</returns>
    public bool TryUseCharge(Entity<BatteryProviderComponent?> provider, EntityUid equipment, float charge)
    {
        if (!Resolve(provider.Owner, ref provider.Comp))
            return false;

        if (!provider.Comp.ConnectedEquipment.Contains(equipment))
            return false; // This equipment is not connected to this provider

        return TryGetBattery((provider, provider.Comp), out var battery) && _batterySystem.TryUseCharge(battery.Value, charge, battery);
    }

    /// <summary>
    /// Attempts to get the amount of charge left from a battery provided by the specified entity.
    /// </summary>
    /// <param name="provider">The entity which may have a battery provider attached.</param>
    /// <param name="charge">Out param for how much charge is stored in the battery. Defaults to 0f in case of failures.</param>
    /// <returns>True if there was a battery attached, false otherwise.</returns>
    public bool TryGetBatteryCharge(Entity<BatteryProviderComponent?> provider, out float charge)
    {
        charge = 0f;

        if (!Resolve(provider.Owner, ref provider.Comp))
            return false;

        if (!TryGetBattery((provider, provider.Comp), out var battery))
            return false;

        charge = battery.Value.Comp.CurrentCharge;
        return true;
    }

    /// <summary>
    /// Attempts to get the battery for a given provider.
    /// </summary>
    /// <param name="provider">The entity which has the battery provider attached.</param>
    /// <param name="battery">Out param for the battery component.</param>
    /// <returns>True if a battery was attached and found, false otherwise.</returns>
    private bool TryGetBattery(Entity<BatteryProviderComponent> provider, [NotNullWhen(true)] out Entity<BatteryComponent>? battery)
    {
        battery = null; // Safety first, null it out

        if (!_powerCellSystem.TryGetBatteryFromSlot(provider, out var uid, out var comp))
            return false; // No power cell in the slot

        battery = new Entity<BatteryComponent>(uid.Value, comp);
        return true;
    }

    /// <summary>
    /// Handles setting the level of battery power on the player's alert screen.
    /// </summary>
    /// <param name="provider">The entity which has the battery provider attached.</param>
    private void SetBatteryPowerAlert(Entity<BatteryProviderComponent> provider)
    {
        var (_, comp) = provider;
        if (comp.Deleted || comp.Wearer == null)
            return;

        if (TryGetBattery(provider, out var batteryEnt))
        {
            var battery = batteryEnt.Value.Comp;
            var severity = ContentHelpers.RoundToLevels(MathF.Max(0f, battery.CurrentCharge), battery.MaxCharge, 8);
            _alertsSystem.ShowAlert(comp.Wearer.Value, comp.PowerAlert, (short)severity);
        }
        else
        {
            _alertsSystem.ClearAlert(comp.Wearer.Value, comp.PowerAlert);
        }
    }

    /// <summary>
    /// Handles when a power cell is inserted into an entity container where a battery provider exists.
    /// </summary>
    /// <param name="provider">The provider where the cell was inserted into.</param>
    /// <param name="args">Args for the event.</param>
    private void OnProviderInsertCellAttempt(Entity<BatteryProviderComponent> provider, ref ContainerIsInsertingAttemptEvent args)
    {
        if (TryComp<PowerCellSlotComponent>(provider, out var slot) && args.Container.ID != slot.CellSlotId)
            return;

        if (!_powerCellSystem.TryGetBatteryFromSlot(provider, out var _, out var _))
            return;

        if (!TryComp<BatteryComponent>(args.EntityUid, out var _))
            args.Cancel();
    }

    /// <summary>
    /// Handles when a power cell is removed from an entity container where a battery provider exists.
    /// </summary>
    /// <param name="provider">The provider where the cell was removed from.</param>
    /// <param name="args">Args for the event.</param>
    private void OnProviderRemoveCellAttempt(Entity<BatteryProviderComponent> provider, ref ContainerIsRemovingAttemptEvent args)
    {
        if (TryComp<PowerCellSlotComponent>(provider, out var slot) && args.Container.ID != slot.CellSlotId)
            return;

        if (!_powerCellSystem.TryGetBatteryFromSlot(provider, out var _, out var _))
            return;

        if (!TryComp<BatteryComponent>(args.EntityUid, out var _))
            args.Cancel();
    }

    /// <summary>
    /// Handles when the battery provider is unequipped by a user.
    /// </summary>
    /// <param name="ent">The entity with a provider attached.</param>
    /// <param name="args">Args for the event, notably the wearer that just has unequipped the provider.</param>
    protected override void OnProviderUnequipped(Entity<BatteryProviderComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        // Ensure the alerts for the wearer are gone when the remove the provider
        _alertsSystem.ClearAlert(args.Wearer, ent.Comp.PowerAlert);
        base.OnProviderUnequipped(ent, ref args);
    }
}
