using Content.Server.Mind;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared._DV.RemoteControl.Components;
using Content.Shared._DV.RemoteControl.Events;
using Content.Shared._DV.RemoteControl.EntitySystems;
using Content.Shared.Pointing;
using Content.Shared.Timing;
using Robust.Server.Audio;
using Robust.Shared.Random;
using Robust.Shared.Map;

namespace Content.Server._DV.RemoteControl.EntitySystems;

/// <summary>
/// Server side handling of Remote Controls
/// </summary>
public sealed partial class RemoteControlSystem : SharedRemoteControlSystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RemoteControlHolderComponent, AfterPointedAtEvent>(OnPointedAtEntity);
        SubscribeLocalEvent<RemoteControlHolderComponent, AfterPointedAtTileEvent>(OnPointedAtTile);

        InitializeReceiverEvents();
    }

    /// <summary>
    /// Sends an order pointing to an entity to receivers on the same channel, or optionally to just
    /// a single bound entity.
    /// </summary>
    /// <param name="holder">Entity holding the remote control.</param>
    /// <param name="pointed">The entity pointed to by the holder.</param>
    /// <param name="recipient">Optional recipient, if set then only that specific entity will receive the order.</param>
    /// <returns>True if the order was sent successfuly, false otherwise.</returns>
    public bool SendEntityPointOrder(Entity<RemoteControlHolderComponent?> holder, EntityUid pointed, EntityUid? recipient = null)
    {
        if (!Resolve(holder, ref holder.Comp))
            return false;

        if (!TryComp<RemoteControlComponent>(holder.Comp.Control, out var controlComp))
            return false;

        var control = (holder.Comp.Control, controlComp);
        if (!CanSendOrder(control))
            return false;

        var ev = new RemoteControlEntityPointOrderEvent(holder, holder.Comp.Control, controlComp.BoundEntities, pointed);
        if (recipient.HasValue)
        {
            RaiseLocalEvent(recipient.Value, ref ev);
            _useDelay.TryResetDelay(holder.Comp.Control);
        }
        else
            SendOrderToReceivers(control, ref ev);

        return true;
    }

    /// <summary>
    /// Sends an order pointing to tile to receivers on the same channel, or optionally to just
    /// a single bound entity.
    /// </summary>
    /// <param name="holder">Entity holding the remote control.</param>
    /// <param name="tile">The map co-ordinates pointed to by the holder.</param>
    /// <param name="recipient">Optional recipient, if set then only that specific entity will receive the order.</param>
    /// <returns>True if the order was sent successfuly, false otherwise.</returns>
    public bool SendTilePointOrder(Entity<RemoteControlHolderComponent?> holder, MapCoordinates tile, EntityUid? recipient = null)
    {
        if (!Resolve(holder, ref holder.Comp))
            return false;

        if (!TryComp<RemoteControlComponent>(holder.Comp.Control, out var controlComp))
            return false;

        var control = (holder.Comp.Control, controlComp);
        if (!CanSendOrder(control))
            return false;

        var ev = new RemoteControlTilePointOrderEvent(holder, holder.Comp.Control, controlComp.BoundEntities, tile);
        if (recipient.HasValue)
        {
            RaiseLocalEvent(recipient.Value, ref ev);
            _useDelay.TryResetDelay(holder.Comp.Control);
        }
        else
            SendOrderToReceivers(control, ref ev);

        return true;
    }

    /// <summary>
    /// Sends an order pointing to the holder to receivers on the same channel, or optionally to just
    /// a single unit.
    /// </summary>
    /// <param name="holder">Entity holding the remote control.</param>
    /// <param name="recipient">Optional recipient, if set then only that specific entity will receive the order.</param>
    /// <returns>True if the order was sent successfuly, false otherwise.</returns>
    public bool SendSelfPointOrder(Entity<RemoteControlHolderComponent?> holder, EntityUid? recipient = null)
    {
        if (!Resolve(holder, ref holder.Comp))
            return false;

        if (!TryComp<RemoteControlComponent>(holder.Comp.Control, out var controlComp))
            return false;

        var control = (holder.Comp.Control, controlComp);
        if (!CanSendOrder(control))
            return false;

        var ev = new RemoteControlSelfPointOrderEvent(holder, holder.Comp.Control, controlComp.BoundEntities);
        if (recipient.HasValue)
        {
            RaiseLocalEvent(recipient.Value, ref ev);
            _useDelay.TryResetDelay(holder.Comp.Control);
        }
        else
            SendOrderToReceivers(control, ref ev);

        return true;
    }

    /// <summary>
    /// Sends an order freeing the bound entities on the same channel, or optionally to just
    /// a single unit.
    /// </summary>
    /// <param name="holder">Entity holding the remote control.</param>
    /// <param name="recipient">Optional recipient, if set then only that specific entity will receive the order.</param>
    /// <returns>True if the order was sent successfuly, false otherwise.</returns>
    public bool SendFreeUnitOrder(Entity<RemoteControlHolderComponent?> holder, EntityUid? recipient = null)
    {
        if (!Resolve(holder, ref holder.Comp))
            return false;

        if (!TryComp<RemoteControlComponent>(holder.Comp.Control, out var controlComp))
            return false;

        var control = (holder.Comp.Control, controlComp);
        if (!CanSendOrder(control))
            return false;

        var ev = new RemoteControlFreeUnitOrderEvent(holder, holder.Comp.Control, controlComp.BoundEntities);
        if (recipient.HasValue)
        {
            RaiseLocalEvent(recipient.Value, ref ev);
            _useDelay.TryResetDelay(holder.Comp.Control);
        }
        else
            SendOrderToReceivers(control, ref ev);

        return true;
    }

    /// <summary>
    /// Handles when an entity using an active remote control points at an entity.
    /// </summary>
    /// <param name="holder">Entity holding/wearing the remote control.</param>
    /// <param name="args">Args for the event, notably the entity pointed at.</param>
    private void OnPointedAtEntity(Entity<RemoteControlHolderComponent> holder, ref AfterPointedAtEvent args)
    {
        if (holder.Owner == args.Pointed)
            SendSelfPointOrder(holder.AsNullable());
        else
            SendEntityPointOrder(holder.AsNullable(), args.Pointed);
    }

    /// <summary>
    /// Handles when an entity using an active remote control points at a tile.
    /// </summary>
    /// <param name="holder">Entity holding/wearing the remote control.</param>
    /// <param name="args">Args for the event, notably the tile pointed at.</param>
    private void OnPointedAtTile(Entity<RemoteControlHolderComponent> holder, ref AfterPointedAtTileEvent args)
    {
        SendTilePointOrder(holder.AsNullable(), args.Pointed);
    }

    /// <summary>
    /// Sends an order from the remote control to all possible Receivers on that channel
    /// </summary>
    /// <typeparam name="T">Type of the order event to send.</typeparam>
    /// <param name="control">Remote control sending this order.</param>
    /// <param name="ev">Order event being sent.</param>
    private void SendOrderToReceivers<T>(Entity<RemoteControlComponent> control, ref T ev) where T : notnull
    {
        var query = EntityQueryEnumerator<RemoteControlReceiverComponent>();
        while (query.MoveNext(out var ent, out var receiverComp))
        {
            if (receiverComp.ChannelName != control.Comp.ChannelName)
                continue; // Not on the same channel, ignore

            RaiseLocalEvent(ent, ref ev);
        }

        _useDelay.TryResetDelay(control.Owner);
    }

    /// <summary>
    /// Checks whether an order can be sent by this remote control, used to limited
    /// the amount of spam one can send to Receivers.
    /// </summary>
    /// <param name="control">Remote control being used.</param>
    /// <returns>True if there is no use delay (cooldown) active for this remote control, false otherwise.</returns>
    private bool CanSendOrder(Entity<RemoteControlComponent> control)
    {
        return !_useDelay.IsDelayed(control.Owner);
    }
}
