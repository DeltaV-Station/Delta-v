using Content.Server._DV.Battery.EntitySystems;
using Content.Server.Actions;
using Content.Server.Popups;
using Content.Shared._DV.Battery.Events;
using Content.Shared._DV.Stunnable.Components;
using Content.Shared._DV.Stunnable.EntitySystems;
using Content.Shared._DV.Stunnable.Events;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Body.Organ;
using Content.Shared.Damage.Events;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Server.Audio;

namespace Content.Server._DV.Stunnable.EntitySystems;

public sealed class K9ShockJawsSystem : SharedK9ShockJawsSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly BatteryProviderSystem _batteryProvider = default!;
    [Dependency] private readonly ItemToggleSystem _itemToggle = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<K9ShockJawsComponent, OrganEnableChangedEvent>(OnOrganEnableChanged);
        SubscribeLocalEvent<K9ShockJawsComponent, BatteryProviderEquippedEvent>(OnProviderEquipped);
        SubscribeLocalEvent<K9ShockJawsComponent, BatteryProviderUnequippedEvent>(OnProviderUnequipped);
        SubscribeLocalEvent<K9ShockJawsComponent, ToggleK9ShockJawsEvent>(OnJawsToggled);

        // Events forwarded from Augment system
        SubscribeLocalEvent<K9ShockJawsComponent, StaminaMeleeHitEvent>(OnStaminaHit);
        // ~Events forwarded from Augment system
    }

    /// <summary>
    /// Handles when a Shock Jaw organ is inserted or removed from an entity, attempting to hook it
    /// up to a battery provider.
    /// </summary>
    /// <param name="ent">Entity being added as an organ.</param>
    /// <param name="args">Args for the event, notably the body the organ was inserted into.</param>
    private void OnOrganEnableChanged(Entity<K9ShockJawsComponent> ent, ref OrganEnableChangedEvent args)
    {
        if (!TryComp<OrganComponent>(ent, out var organ) || organ.Body is not { } body)
            return;

        if (args.Enabled)
            _batteryProvider.AddConnectedEquipment(body, ent, out ent.Comp.BatteryProvider);
        else
            _batteryProvider.RemoveConnectedEquipment(body, ent);
    }

    /// <summary>
    /// Handles when a user successfully makes a stamina hit against a target, allowing the shock
    /// jaws to modify the stamina damage if we are active.
    /// N.b. This event is forwarded from the Augment system.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="args"></param>
    private void OnStaminaHit(Entity<K9ShockJawsComponent> ent, ref StaminaMeleeHitEvent args)
    {
        if (!ent.Comp.Active)
            return; // Not active

        // Attempt to draw power from our battery provider, if we have one.
        if (!ent.Comp.BatteryProvider.HasValue ||
            !_batteryProvider.TryUseCharge(ent.Comp.BatteryProvider.Value, ent, ent.Comp.ChargePerHit))
        {
            // If there is no power available, toggle jaws off and don't alter the stamina damage done.
            ToggleJaws(ent, false);
            return;
        }

        args.FlatModifier += ent.Comp.FlatModifier;
    }

    /// <summary>
    /// Handles when a battery provider has been equipped by a user, allowing the jaws to connect to it
    /// for drawing power.
    /// </summary>
    /// <param name="ent">Shock jaws entity that can be connected to.</param>
    /// <param name="args">Args for the event, notably the connected equipment hashset.</param>
    private void OnProviderEquipped(Entity<K9ShockJawsComponent> ent, ref BatteryProviderEquippedEvent args)
    {
        if (ent.Comp.BatteryProvider.HasValue)
            return; // No need to override, assume current battery provider is correct.

        args.ConnectedEquipment.Add(ent);
        ent.Comp.BatteryProvider = args.Provider;
    }

    /// <summary>
    /// Handles when a battery provider has been unequipped by a user.
    /// Will attempt another connection to any battery providers if possible.
    /// </summary>
    /// <param name="ent">Entity, which has jaws attached, where a battery provider is being unequipped.</param>
    /// <param name="args">Args for the event, notably which provider has been removed.</param>
    private void OnProviderUnequipped(Entity<K9ShockJawsComponent> ent, ref BatteryProviderUnequippedEvent args)
    {
        if (!ent.Comp.BatteryProvider.HasValue || ent.Comp.BatteryProvider != args.Provider)
            return; // We didn't have a provider or the one removed wasn't one we care about

        // Battery provider we care about is gone.
        // Ensure full cleanup of the state, deactivate, toggle, etc.
        ToggleJaws(ent, false);
        ent.Comp.BatteryProvider = null;

        // Attempt to connect to any other provider that the user is wearing
        _batteryProvider.AddConnectedEquipment(ent.Owner, ent);
    }

    /// <summary>
    /// Handles when the user activates the toggle action for the jaws, setting them to on or off.
    /// </summary>
    /// <param name="ent">Entity which has toggled the action.</param>
    /// <param name="args">Args for the event.</param>
    private void OnJawsToggled(Entity<K9ShockJawsComponent> ent, ref ToggleK9ShockJawsEvent args)
    {
        if (!ent.Comp.BatteryProvider.HasValue)
        {
            _popup.PopupEntity(Loc.GetString("augment-k9-shockjaws-no-provider"), args.Performer, args.Performer);
            _audio.PlayPvs(ent.Comp.SoundFailToActivate, args.Performer);
            return;
        }

        if (!_batteryProvider.TryGetBatteryCharge(ent.Comp.BatteryProvider.Value, out var charge) ||
            charge < ent.Comp.ChargePerHit)
        {
            _popup.PopupEntity(Loc.GetString("augment-k9-shockjaws-low-charge"), args.Performer, args.Performer);
            _audio.PlayPvs(ent.Comp.SoundFailToActivate, args.Performer);
            return;
        }

        ToggleJaws(ent, !ent.Comp.Active);
        args.Handled = true;
    }

    /// <summary>
    /// Internal function for toggling and updating the effects the shock jaws makes use of.
    /// </summary>
    /// <param name="ent">The shock jaws entity to toggle.</param>
    /// <param name="isActive">True if the jaws should be active, false if they should be inactive.</param>
    private void ToggleJaws(Entity<K9ShockJawsComponent> ent, bool isActive)
    {
        if (!TryComp<OrganComponent>(ent, out var organ) || organ.Body is not { } body)
            return; // This needs to be attached to a body in order to function

        ent.Comp.Active = isActive;

        _actions.SetToggled(ent.Comp.ActionEntity, isActive);
        _itemToggle.TrySetActive((ent, null), isActive); // Play the sound effects from the jaws activating.

        // The melee toggle is on the actual body the jaws are attached to, not the jaws themselves.
        var ev = new ItemToggledEvent(true, isActive, ent.Owner);
        RaiseLocalEvent(body, ref ev);
    }
}
