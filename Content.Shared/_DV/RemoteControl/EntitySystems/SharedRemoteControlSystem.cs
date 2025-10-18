using Content.Shared._DV.RemoteControl.Components;
using Content.Shared._DV.RemoteControl.Events;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Hands;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._DV.RemoteControl.EntitySystems;

/// <summary>
/// Shared logic for the remote control order system
/// </summary>
public abstract class SharedRemoteControlSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RemoteControlComponent, ToggleRemoteControlEvent>(OnRemoteControlToggled);
        SubscribeLocalEvent<RemoteControlComponent, GetItemActionsEvent>(OnGetRemoteControlActions);

        SubscribeLocalEvent<RemoteControlComponent, GotEquippedEvent>(OnRemoteControlEquipped);
        SubscribeLocalEvent<RemoteControlComponent, GotUnequippedEvent>(OnRemoteControlUnequipped);
    }

    /// <summary>
    /// Binds an entity to a remote control.
    /// </summary>
    /// <param name="control">The control the bind to.</param>
    /// <param name="entity">The entity to bind.</param>
    public void BindEntity(Entity<RemoteControlComponent?> control, EntityUid entity)
    {
        if (!Resolve(control, ref control.Comp))
            return;

        control.Comp.BoundNPCs.Add(entity);
    }

    /// <summary>
    /// Binds an entity to a remote control.
    /// </summary>
    /// <param name="control">The control the bind to.</param>
    /// <param name="entity">The entity to bind.</param>
    public bool UnbindEntity(Entity<RemoteControlComponent?> control, EntityUid entity)
    {
        if (!Resolve(control, ref control.Comp))
            return false;

        if (!control.Comp.BoundNPCs.Remove(entity))
            return false;

        return true;
    }

    /// <summary>
    /// Handles when a remote control is equipped during map init of an parent entity and attempts to bind
    /// the remote control to that entity.
    /// </summary>
    /// <param name="control">The remote control being equipped.</param>
    /// <param name="args">Args for the event, notably the equipee.</param>
    private void OnRemoteControlEquipped(Entity<RemoteControlComponent> control, ref GotEquippedEvent args)
    {
        if (MetaData(args.Equipee).EntityLifeStage != EntityLifeStage.MapInitialized &&
            !HasComp<RemoteControlRecieverComponent>(args.Equipee))
            return;

        BindEntity(control.AsNullable(), args.Equipee);
    }

    /// <summary>
    /// Handles when a player uses the toggle action for the remote control and updates the state.
    /// </summary>
    /// <param name="control">Remote control that was toggled.</param>
    /// <param name="args">Args for the event, notably the performer.</param>
    private void OnRemoteControlToggled(Entity<RemoteControlComponent> control, ref ToggleRemoteControlEvent args)
    {
        if (control.Comp.ToggleActionEntid == null || _timing.ApplyingState)
            return;

        if (!TryComp<ActionComponent>(control.Comp.ToggleActionEntid, out var actionComp))
            return;

        var newState = !actionComp.Toggled;

        string msg;
        if (newState)
        {
            EnsureComp<RemoteControlHolderComponent>(args.Performer, out var holderComp);
            holderComp.Control = control;
            msg = $"{control.Comp.ToggleActionBase}-up";
        }
        else
        {
            RemComp<RemoteControlHolderComponent>(args.Performer);
            msg = $"{control.Comp.ToggleActionBase}-down";
        }

        if (_net.IsServer)
        {
            Popup.PopupEntity(Loc.GetString(msg, ("user", args.Performer)), args.Performer, args.Performer);
        }

        args.Toggle = true;
        args.Handled = true;
    }

    /// <summary>
    /// Handles when a remote control is equipped and the inventory system queries for any actions associated with it.
    /// </summary>
    /// <param name="control">Remote control that was equipped.</param>
    /// <param name="args">Args for the event.</param>
    private void OnGetRemoteControlActions(Entity<RemoteControlComponent> control, ref GetItemActionsEvent args)
    {
        args.AddAction(ref control.Comp.ToggleActionEntid, control.Comp.ToggleAction);
        Dirty(control);
    }

    /// <summary>
    /// Handles when a remote control is unequipped by an entity, clearing up any toggles and other components.
    /// </summary>
    /// <param name="control">Remote control that was unequipped.</param>
    /// <param name="args">Args for the event, notably who unequipped it.</param>
    private void OnRemoteControlUnequipped(Entity<RemoteControlComponent> control, ref GotUnequippedEvent args)
    {
        // Ensure the action and equipee are cleaned up
        if (control.Comp.ToggleActionEntid != null)
            _actions.SetToggled(control.Comp.ToggleActionEntid, false);

        RemComp<RemoteControlHolderComponent>(args.Equipee);
    }
}
