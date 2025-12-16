using System.Linq;
using Content.Shared._DV.RemoteControl.Components;
using Content.Shared._DV.RemoteControl.Events;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._DV.RemoteControl.EntitySystems;

/// <summary>
/// Shared logic for the remote control order system
/// </summary>
public abstract class SharedRemoteControlSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RemoteControlComponent, ToggleRemoteControlEvent>(OnRemoteControlToggled);
        SubscribeLocalEvent<RemoteControlComponent, GetItemActionsEvent>(OnGetRemoteControlActions);

        SubscribeLocalEvent<RemoteControlComponent, GotUnequippedEvent>(OnRemoteControlUnequipped);

        SubscribeLocalEvent<RemoteControlComponent, GotUnequippedHandEvent>(OnRemoteControlHandUnequipped);

        SubscribeLocalEvent<RemoteControlComponent, GetVerbsEvent<UtilityVerb>>(OnUtilityVerb);
        SubscribeLocalEvent<RemoteControlComponent, RemoteControlBindChangeDoAfterEvent>(OnBindChangeDoAfter);
    }

    /// <summary>
    /// Binds an entity to a remote control.
    /// </summary>
    /// <param name="control">The control the bind to.</param>
    /// <param name="entity">The entity to bind.</param>
    public void BindEntity(Entity<RemoteControlComponent?> control, EntityUid entity)
    {
        if (!Resolve(control, ref control.Comp) ||
            !TryComp<RemoteControlReceiverComponent>(entity, out var receiverComponent))
            return;

        if (!control.Comp.AllowMultiple && control.Comp.BoundEntities.Count > 0)
        {
            while (control.Comp.BoundEntities.Count > 0)
            {
                if (!UnbindEntity(control, control.Comp.BoundEntities.First()))
                    break;
            }
        }

        if (!control.Comp.BoundEntities.Contains(entity))
        {
            control.Comp.BoundEntities.Add(entity);
            receiverComponent.BoundController = control;
        }
    }

    /// <summary>
    /// Unbinds an entity to a remote control.
    /// </summary>
    /// <param name="control">The control the unbind from.</param>
    /// <param name="entity">The entity to unbind.</param>
    /// <returns>True if the entity was unbound from the control, false otherwise.</returns>
    public bool UnbindEntity(Entity<RemoteControlComponent?> control, EntityUid entity)
    {
        if (!Resolve(control, ref control.Comp) ||
            !TryComp<RemoteControlReceiverComponent>(entity, out var receiverComponent))
            return false;

        if (receiverComponent.BoundController != control)
            return false; // This entity is not bound to this controller

        if (!control.Comp.BoundEntities.Remove(entity))
            return false;

        receiverComponent.BoundController = null;

        SetUnitFree((entity, receiverComponent));

        return true;
    }

    /// <summary>
    /// Sets a unit free, only implemented on server.
    /// </summary>
    /// <param name="entity">The entity to set free.</param>
    protected abstract void SetUnitFree(Entity<RemoteControlReceiverComponent> entity);

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
        if (control.Comp.BoundEntities.Contains(args.User))
            return; // Bound entities don't get to control themselves.

        // As a measure against entities having their own remote control, we don't allow a receiver with
        // the same channel name as the control to get an action.
        if (TryComp<RemoteControlReceiverComponent>(args.User, out var receiverComp) &&
            receiverComp.ChannelName == control.Comp.ChannelName)
            return;

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

    /// <summary>
    /// Handles when a remote control is unequipped from a hand, clearing up any toggles and other components.
    /// </summary>
    /// <param name="control">Remote control that was unequipped.</param>
    /// <param name="args">Args for the event, notably who unequipped it.</param>
    private void OnRemoteControlHandUnequipped(Entity<RemoteControlComponent> control, ref GotUnequippedHandEvent args)
    {
        // Ensure the action and equipee are cleaned up
        if (control.Comp.ToggleActionEntid != null)
            _actions.SetToggled(control.Comp.ToggleActionEntid, false);

        RemComp<RemoteControlHolderComponent>(args.User);
    }

    /// <summary>
    /// Sets up a binding change doafter at the users' request, either binding or unbinding an entity
    /// to a control.
    /// </summary>
    /// <param name="control">The control to bind to or unbind from.</param>
    /// <param name="user">The current user of the control.</param>
    /// <param name="target">The target to bind/unbind.</param>
    /// <param name="binding">True if we are binding, false otherwise.</param>
    private void AttemptStartBindChange(Entity<RemoteControlComponent> control, EntityUid user, EntityUid target, bool binding)
    {
        string message;
        TimeSpan doAfterTime;

        if (binding)
        {
            message = "remote-control-bind-popup-start";
            doAfterTime = control.Comp.BindingTime;
        }
        else
        {
            message = "remote-control-unbind-popup-start";
            doAfterTime = control.Comp.UnbindingTime;
        }


        Popup.PopupPredicted(Loc.GetString(message, ("user", user), ("target", target)),
            user,
            user,
            PopupType.Medium);

        var doargs = new DoAfterArgs(
            EntityManager,
            user,
            doAfterTime,
            new RemoteControlBindChangeDoAfterEvent(binding),
            control,
            target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doargs);
    }

    /// <summary>
    /// Handles when a doafter for a binding change has occured, actually performing the binding/unbinding
    /// as requested.
    /// </summary>
    /// <param name="control">The control which received the event.</param>
    /// <param name="args">Args for the event, notably the target and whether to bind or not.</param>
    private void OnBindChangeDoAfter(Entity<RemoteControlComponent> control, ref RemoteControlBindChangeDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target is not { } target)
            return;

        if (args.Binding)
            BindEntity(control.AsNullable(), target);
        else
            UnbindEntity(control.AsNullable(), target);
    }

    /// <summary>
    /// Handles when the verb system requests extra verbs for a remote control.
    /// </summary>
    /// <param name="control">The control being requested for.</param>
    /// <param name="args">Args for the event.</param>
    private void OnUtilityVerb(Entity<RemoteControlComponent> control, ref GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanInteract
            || !TryComp<RemoteControlReceiverComponent>(args.Target, out var targetReceiver)
            || targetReceiver.ChannelName != control.Comp.ChannelName)
            return; // Not a receiver to be bound to, or not on the right channel

        var user = args.User;
        var target = args.Target;

        if (control.Comp.BoundEntities.Contains(target))
        {
            var unbindVerb = new UtilityVerb()
            {
                Act = () => AttemptStartBindChange(control, user, target, binding: false),
                Icon = new SpriteSpecifier.Rsi(new("/Textures/Objects/Specific/Medical/Surgery/scalpel.rsi/"), "scalpel"),
                Text = Loc.GetString("remote-control-unbind-verb-text"),
                Message = Loc.GetString("remote-control-unbind-verb-message"),
                DoContactInteraction = true
            };
            args.Verbs.Add(unbindVerb);
        }
        else
        {
            var bindVerb = new UtilityVerb()
            {
                Act = () => AttemptStartBindChange(control, user, target, binding: true),
                Icon = new SpriteSpecifier.Rsi(new("/Textures/Objects/Specific/Medical/Surgery/scalpel.rsi/"), "scalpel"),
                Text = Loc.GetString("remote-control-bind-verb-text"),
                Message = Loc.GetString("remote-control-bind-verb-message"),
                DoContactInteraction = true
            };
            args.Verbs.Add(bindVerb);
        }
    }
}
