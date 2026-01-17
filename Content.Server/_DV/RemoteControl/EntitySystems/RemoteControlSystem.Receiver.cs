using System.Numerics;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Shared._DV.RemoteControl.Components;
using Content.Shared._DV.RemoteControl.EntitySystems;
using Content.Shared._DV.RemoteControl.Events;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._DV.RemoteControl.EntitySystems;

/// <summary>
/// Server side handling of Remote Controls.
/// Specifically handling recieving orders.
/// </summary>
public sealed partial class RemoteControlSystem : SharedRemoteControlSystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    private readonly string _targetKey = "TargetCoordinates";
    private readonly LocId _defaultLocString = "remote-control-order-default";

    /// <summary>
    /// Initializes event subscriptions for handling orders.
    /// </summary>
    private void InitializeReceiverEvents()
    {
        SubscribeLocalEvent<RemoteControlReceiverComponent, ComponentStartup>(OnComponentStartup);

        SubscribeLocalEvent<RemoteControlReceiverComponent, RemoteControlEntityPointOrderEvent>(OnEntityPointOrder);
        SubscribeLocalEvent<RemoteControlReceiverComponent, RemoteControlTilePointOrderEvent>(OnTilePointOrder);
        SubscribeLocalEvent<RemoteControlReceiverComponent, RemoteControlSelfPointOrderEvent>(OnSelfPointOrder);
        SubscribeLocalEvent<RemoteControlReceiverComponent, RemoteControlFreeUnitOrderEvent>(OnFreeUnitOrder);
    }

    /// <summary>
    /// Handles when the Receiver component starts up, setting the unit to "Free control" immediately.
    /// </summary>
    /// <param name="ent">The entity where the component is starting up.</param>
    /// <param name="args">Args for the event.</param>
    private void OnComponentStartup(Entity<RemoteControlReceiverComponent> ent, ref ComponentStartup args)
    {
        SetUnitFree(ent);
    }

    /// <summary>
    /// Handles when a entity has been pointed to by an active remote control user.
    /// </summary>
    /// <param name="ent">Entity recieving the order.</param>
    /// <param name="args">Args for the event, notably sound and origin.</param>
    private void OnEntityPointOrder(Entity<RemoteControlReceiverComponent> ent, ref RemoteControlEntityPointOrderEvent args)
    {
        var target = args.Target;
        TryHandleOrder(
            ent,
            RemoteControlOrderType.EntityPoint,
            args,
            () =>
            {
                _npc.SetBlackboard(ent, NPCBlackboard.CurrentOrderedTarget, target);
                _npc.SetBlackboard(ent, _targetKey, new EntityCoordinates(target, Vector2.Zero));
            }
        );
    }

    /// <summary>
    /// Handles when a tile has been pointed to by an active remote control user.
    /// </summary>
    /// <param name="ent">Entity recieving the order.</param>
    /// <param name="args">Args for the event, notably sound and origin.</param>
    private void OnTilePointOrder(Entity<RemoteControlReceiverComponent> ent, ref RemoteControlTilePointOrderEvent args)
    {
        var grid = _transform.GetGrid(ent.Owner);
        if (!grid.HasValue)
            return;

        var location = _transform.ToCoordinates((grid.Value, null), args.Location);
        TryHandleOrder(
            ent,
            RemoteControlOrderType.TilePoint,
            args,
            () =>
            {
                _npc.SetBlackboard(ent, NPCBlackboard.MovementTarget, location);
            }
        );
    }

    /// <summary>
    /// Handles when the user of an active remote control has pointed to themselves.
    /// </summary>
    /// <param name="ent">Entity recieving the order.</param>
    /// <param name="args">Args for the event, notably sound and origin.</param>
    private void OnSelfPointOrder(Entity<RemoteControlReceiverComponent> ent, ref RemoteControlSelfPointOrderEvent args)
    {
        var origin = args.User;
        TryHandleOrder(
            ent,
            RemoteControlOrderType.SelfPoint,
            args,
            () =>
            {
                _npc.SetBlackboard(ent,
                    NPCBlackboard.FollowTarget,
                    new EntityCoordinates(origin, Vector2.Zero));
            }
        );
    }

    /// <summary>
    /// Handles when the user of an active remote control returns bound entities to freedom.
    /// I.e. They return to a normal, non-controlled, state.
    /// </summary>
    /// <param name="ent">Entity recieving the order.</param>
    /// <param name="args">Args for the event, notably sound and origin.</param>
    private void OnFreeUnitOrder(Entity<RemoteControlReceiverComponent> ent, ref RemoteControlFreeUnitOrderEvent args)
    {
        TryHandleOrder(
            ent,
            RemoteControlOrderType.FreeUnit,
            args,
            () => { }
        );
    }

    /// <summary>
    /// Attempts to handle an order given to this receiever.
    /// </summary>
    /// <param name="receiver">The entity who has received the order.</param>
    /// <param name="orderType">The type of order received.</param>
    /// <param name="args">The args for the event.</param>
    /// <param name="orderAction">A function for providing NPC instructions.</param>
    private void TryHandleOrder(
        Entity<RemoteControlReceiverComponent> receiver,
        RemoteControlOrderType orderType,
        BaseRemoteControlOrderEvent args,
        Action orderAction
    )
    {
        if (args.User == receiver.Owner)
            return; // Don't let users attempt to control themselves

        if (!TryComp<RemoteControlComponent>(args.Control, out var controlComp) ||
            controlComp.ChannelName != receiver.Comp.ChannelName)
            return;

        if (!IsNPC(receiver))
        {
            HandleOrderEffects(receiver, args.User, (args.Control, controlComp), orderType);
            return;
        }

        if (receiver.Comp.BoundControl != args.Control)
            return; // This isn't the control we think we're bound to

        if (!args.BoundEntities.Contains(receiver.Owner))
            return; // This order is not for this entity

        if (!receiver.Comp.CanUnderstand)
            return;

        UpdateNPCOrders(
            receiver,
            orderType,
            orderAction
        );
    }

    /// <summary>
    /// Server side implementation of setting a unit free.
    /// </summary>
    /// <param name="entity">The entity to set free.</param>
    protected override void SetUnitFree(Entity<RemoteControlReceiverComponent> entity)
    {
        UpdateNPCOrders(entity,
            RemoteControlOrderType.FreeUnit,
            () => { }
        );
    }

    /// <summary>
    /// Checks whether the entity is an NPC or a player.
    /// </summary>
    /// <param name="ent">Entity to check for NPC status.</param>
    /// <returns>True if they are an NPC, false otherwise.</returns>
    private bool IsNPC(EntityUid ent)
    {
        return !_mind.TryGetMind(ent, out _, out var mindComponent) || // No mind
               !_player.TryGetSessionById(mindComponent.UserId, out var session) ||
               session.AttachedEntity != ent; // Had a mind, but not attached the requested entity
    }

    /// <summary>
    /// Updates the NPCs current orders and runs the supplied action to populate any blackboard
    /// details that are required.
    /// </summary>
    /// <param name="npc">NPC to update orders for.</param>
    /// <param name="order">The current order enum they should follow.</param>
    /// <param name="orderAction">A lambda that updates any blackboard information for the current order.</param>
    private void UpdateNPCOrders(EntityUid npc, RemoteControlOrderType order, Action orderAction)
    {
        _npc.SetBlackboard(npc, NPCBlackboard.CurrentOrders, order);

        if (!TryComp<HTNComponent>(npc, out var htn))
            return;

        if (htn.Plan != null)
            _htn.ShutdownPlan(htn);

        orderAction();
        _htn.Replan(htn);
    }

    /// <summary>
    /// Handles producing any general audio or visual effects the order has.
    /// Schedules sounds to be played and any popups that might need to be sent.
    /// </summary>
    /// <param name="ent">Entity recieving this order.</param>
    /// <param name="user">The user of the remote control.</param>
    /// <param name="control">The remote control entity that sent this order.</param>
    /// <param name="orderString">The localisation string to use for this order.</param>
    private void HandleOrderEffects(Entity<RemoteControlReceiverComponent> ent,
        EntityUid user,
        Entity<RemoteControlComponent> control,
        RemoteControlOrderType orderType)
    {
        var audioParams = new AudioParams
        {
            MaxDistance = 300f, // Long distance
            RolloffFactor = 0f // Hearable at the same volume across the station
        };

        string message;
        if (ent.Comp.CanUnderstand)
        {
            message = Loc.GetString(ent.Comp.OrderStrings.GetValueOrDefault(orderType, _defaultLocString));
        }
        else
        {

            // Lower the volume by a bunch so we don't overwhelm players that don't need to understand this
            audioParams.Volume = -10f;
            message = Loc.GetString(_random.Pick(control.Comp.Screeches));
        }

        _audio.PlayEntity(_audio.ResolveSound(control.Comp.UseSound), ent, user, audioParams);
        Popup.PopupEntity(message, ent, ent, PopupType.Medium);
    }
}
