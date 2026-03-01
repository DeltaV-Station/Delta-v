using System.Linq;
using Content.Shared._DV.RemoteControl.Components;
using Content.Shared._DV.RemoteControl.Events;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Inventory.Events;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._DV.RemoteControl.EntitySystems;

/// <summary>
/// Shared logic for the remote control order system
/// </summary>
public abstract class SharedRemoteControlSystem : EntitySystem
{
    [Dependency] protected readonly SharedActionsSystem Actions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] protected readonly UseDelaySystem UseDelay = default!;
    [Dependency] private readonly ItemToggleSystem _itemToggle = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RemoteControlComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<RemoteControlComponent, ItemToggleActivateAttemptEvent>(OnToggleActivateAttempt);
        SubscribeLocalEvent<RemoteControlComponent, ItemToggledEvent>(OnToggled);

        SubscribeLocalEvent<RemoteControlComponent, GotUnequippedEvent>(OnRemoteControlUnequipped);

        SubscribeLocalEvent<RemoteControlComponent, GotUnequippedHandEvent>(OnRemoteControlHandUnequipped);

        SubscribeLocalEvent<RemoteControlComponent, GetVerbsEvent<UtilityVerb>>(OnUtilityVerb);
        SubscribeLocalEvent<RemoteControlComponent, RemoteControlBindChangeDoAfterEvent>(OnBindChangeDoAfter);
    }

    private void OnMapInit(Entity<RemoteControlComponent> ent, ref MapInitEvent args)
    {
        EnsureComp<UseDelayComponent>(ent);
        UseDelay.SetLength(ent.Owner, ent.Comp.Cooldown);
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

        if (receiverComponent.BoundControl is { } existingControl)
        {
            if (existingControl == control.Owner)
                return; // We're already bound to this control

            if (!UnbindEntity((existingControl, null), entity))
                return; // Remove this entity from the existing controls' bound entities
        }

        if (!control.Comp.AllowMultiple && control.Comp.BoundEntities.Count > 0)
        {
            foreach (var boundEntity in control.Comp.BoundEntities.ToList())
            {
                if (!UnbindEntity(control, boundEntity))
                    Log.Warning($"Unable to unbind [{boundEntity}] from [{control.Owner}]");
            }

            if (control.Comp.BoundEntities.Count > 0)
                return; // Failed to unbind, don't bother trying to bind something new
        }

        if (!control.Comp.BoundEntities.Contains(entity))
        {
            control.Comp.BoundEntities.Add(entity);
            receiverComponent.BoundControl = control;
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

        if (receiverComponent.BoundControl != control)
            return false; // This entity is not bound to this controller

        if (!control.Comp.BoundEntities.Remove(entity))
            return false;

        receiverComponent.BoundControl = null;

        SetUnitFree((entity, receiverComponent));

        if (control.Comp.BoundEntities.Count == 0)
        {
            // If we've got no entities to control, we should just turn off.
            _itemToggle.TryDeactivate(control.Owner);
        }

        return true;
    }

    /// <summary>
    /// Sets a unit free, only implemented on server.
    /// </summary>
    /// <param name="entity">The entity to set free.</param>
    protected abstract void SetUnitFree(Entity<RemoteControlReceiverComponent> entity);

    /// <summary>
    /// Handles when a player attempts to toggle the remote control.
    /// </summary>
    /// <param name="control">Remote control that is attempting to be toggled.</param>
    /// <param name="args">Args for the event, notably the performer.</param>
    private void OnToggleActivateAttempt(Entity<RemoteControlComponent> control, ref ItemToggleActivateAttemptEvent args)
    {
        if (control.Comp.BoundEntities.Count == 0)
        {
            args.Cancelled = true;
            args.Popup = Loc.GetString(control.Comp.NoBoundEntitiesWarning);
        }
    }

    /// <summary>
    /// Handles when a remote control was toggled by a player.
    /// </summary>
    /// <param name="control">Remote control that has been toggled.</param>
    /// <param name="args">Args for the event.</param>
    private void OnToggled(Entity<RemoteControlComponent> control, ref ItemToggledEvent args)
    {
        if (!_container.TryGetContainingContainer((control.Owner, null, null), out var container))
            return;

        if (args.Activated)
        {
            EnsureComp<RemoteControlHolderComponent>(container.Owner, out var holderComp);
            holderComp.Control = control;
        }
        else
        {
            RemComp<RemoteControlHolderComponent>(container.Owner);
        }
    }

    /// <summary>
    /// Handles when a remote control is unequipped by an entity, clearing up any toggles and other components.
    /// </summary>
    /// <param name="control">Remote control that was unequipped.</param>
    /// <param name="args">Args for the event, notably who unequipped it.</param>
    private void OnRemoteControlUnequipped(Entity<RemoteControlComponent> control, ref GotUnequippedEvent args)
    {
        _itemToggle.TryDeactivate(control.Owner, args.Equipee);
        RemComp<RemoteControlHolderComponent>(args.Equipee);
    }

    /// <summary>
    /// Handles when a remote control is unequipped from a hand, clearing up any toggles and other components.
    /// </summary>
    /// <param name="control">Remote control that was unequipped.</param>
    /// <param name="args">Args for the event, notably who unequipped it.</param>
    private void OnRemoteControlHandUnequipped(Entity<RemoteControlComponent> control, ref GotUnequippedHandEvent args)
    {
        _itemToggle.TryDeactivate(control.Owner, args.User);
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

        if (!targetReceiver.Bindable)
            return; // Not a target that can be bound to a remote control.

        var user = args.User;
        var target = args.Target;

        if (control.Comp.BoundEntities.Contains(target))
        {
            var unbindVerb = new UtilityVerb()
            {
                Act = () => AttemptStartBindChange(control, user, target, binding: false),
                Icon = new SpriteSpecifier.Texture(new("/Textures/_DV/Interface/VerbIcons/remote_control_unbind.png")),
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
                Icon = new SpriteSpecifier.Texture(new("/Textures/_DV/Interface/VerbIcons/remote_control_bind.png")),
                Text = Loc.GetString("remote-control-bind-verb-text"),
                Message = Loc.GetString("remote-control-bind-verb-message"),
                DoContactInteraction = true
            };
            args.Verbs.Add(bindVerb);
        }
    }
}
