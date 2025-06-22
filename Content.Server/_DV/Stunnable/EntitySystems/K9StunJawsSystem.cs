using Content.Server._DV.Battery.EntitySystems;
using Content.Shared._DV.Battery.Events;
using Content.Shared._DV.Stunnable.Components;
using Content.Shared._DV.Stunnable.EntitySystems;
using Content.Shared.Damage.Events;

namespace Content.Server._DV.Stunnable.EntitySystems;

public sealed partial class K9StunJawsSystem : SharedK9StunJawsSystem
{
    [Dependency] private readonly BatteryProviderSystem _batteryProviderSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<K9StunJawsComponent, StaminaDamageOnHitAttemptEvent>(OnStaminaHitAttempt);

        SubscribeLocalEvent<K9StunJawsComponent, BatteryProviderEquippedEvent>(OnProviderEquipped);
        SubscribeLocalEvent<K9StunJawsComponent, BatteryProviderUnequippedEvent>(OnProviderUnequipped);
    }

    /// <summary>
    /// Handles when the component starts on an entity.
    /// Calls the base component startup, and finds any attached battery providers on the wearer.
    /// </summary>
    /// <param name="ent">The entity which has had the jaws component attached.</param>
    /// <param name="args">Args for the event.</param>
    protected override void OnComponentStartup(Entity<K9StunJawsComponent> ent, ref ComponentStartup args)
    {
        base.OnComponentStartup(ent, ref args);

        _batteryProviderSystem.AddConnectedEquipment(ent.Owner, ent, out ent.Comp.BatteryProvider);
    }

    /// <summary>
    /// Handles when the user makes an attack attempt on a user that does stamina damage.
    /// Will cancel the stamina damage hit if there is not enough charge for the battery.
    /// </summary>
    /// <param name="ent">Entity which is making a stmina attack attempt.</param>
    /// <param name="args">Args for the event.</param>
    private void OnStaminaHitAttempt(Entity<K9StunJawsComponent> ent, ref StaminaDamageOnHitAttemptEvent args)
    {
        if (!ent.Comp.Active)
        {
            args.Cancelled = true;
            return;
        }

        if (!ent.Comp.BatteryProvider.HasValue ||
            !_batteryProviderSystem.TryUseCharge(ent.Comp.BatteryProvider.Value, ent, ent.Comp.ChargePerHit))
        {
            ToggleJawsDamage(ent, false);
            args.Cancelled = true;
        }
    }

    /// <summary>
    /// Handles when a battery provider has been equipped by a user, allowing the jaws to connect to it
    /// for drawing power.
    /// </summary>
    /// <param name="ent">Entity, which has jaws attached, where a battery provider is being equipped.</param>
    /// <param name="args">Args for the event, notably the connected equipment hashset.</param>
    private void OnProviderEquipped(Entity<K9StunJawsComponent> ent, ref BatteryProviderEquippedEvent args)
    {
        if (ent.Comp.BatteryProvider.HasValue)
            return; // No need to override, assume current battery provider is correct.

        args.ConnectedEquipment.Add(ent);
        ent.Comp.BatteryProvider = args.Item;
    }

    /// <summary>
    /// Handles when a battery provider has been unequipped by a user.
    /// Will attempt another connection to any battery providers if possible.
    /// </summary>
    /// <param name="ent">Entity, which has jaws attached, where a battery provider is being unequipped.</param>
    /// <param name="args">Args for the event, notably which provider has been removed.</param>
    private void OnProviderUnequipped(Entity<K9StunJawsComponent> ent, ref BatteryProviderUnequippedEvent args)
    {
        if (!ent.Comp.BatteryProvider.HasValue || ent.Comp.BatteryProvider != args.Item)
            return;

        // Ensure full cleanup of the state, deactivate, toggle, etc.
        ent.Comp.Active = false;
        ent.Comp.BatteryProvider = null;
        ToggleJawsDamage(ent, false);
        ActionSystem.SetToggled(ent.Comp.ActionEntity, false);

        // Attempt to connect to any other provider that the user is wearing
        _batteryProviderSystem.AddConnectedEquipment(ent.Owner, ent);
    }
}
