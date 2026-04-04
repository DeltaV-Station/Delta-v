using Content.Server.Mind;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared._DV.RemoteControl.Components;
using Content.Shared._DV.RemoteControl.Events;
using Content.Shared._DV.RemoteControl.EntitySystems;
using Content.Shared.Pointing;
using Robust.Server.Audio;
using Robust.Shared.Random;
using Robust.Shared.Map;
using System.Numerics;
using Content.Shared.Clothing.Components;

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
        return TrySendOrder(
            holder,
            RemoteControlOrderType.EntityPoint,
            new EntityCoordinates(pointed, Vector2.Zero),
            (control) => new RemoteControlEntityPointOrderEvent(holder, control.Owner, control.Comp.BoundEntities, pointed),
            recipient
        );
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
        var grid = _transform.GetGrid(holder.Owner);
        if (!grid.HasValue)
            return false;

        var location = _transform.ToCoordinates((grid.Value, null), tile);
        return TrySendOrder(
            holder,
            RemoteControlOrderType.TilePoint,
            location,
            (control) => new RemoteControlTilePointOrderEvent(holder, control.Owner, control.Comp.BoundEntities, tile),
            recipient
        );
    }

    /// <summary>
    /// Sends an order pointing to the holder to receivers on the same channel, or optionally to just
    /// a single unit.
    /// </summary>
    /// <param name="holder">Entity holding the remote control.</param>
    /// <param name="pointed">The entity pointed to by the holder.</param>
    /// <param name="recipient">Optional recipient, if set then only that specific entity will receive the order.</param>
    /// <returns>True if the order was sent successfuly, false otherwise.</returns>
    public bool SendSelfPointOrder(Entity<RemoteControlHolderComponent?> holder, EntityUid pointed, EntityUid? recipient = null)
    {
        return TrySendOrder(
            holder,
            RemoteControlOrderType.SelfPoint,
            new EntityCoordinates(pointed, Vector2.Zero),
            (control) => new RemoteControlSelfPointOrderEvent(holder, control.Owner, control.Comp.BoundEntities),
            recipient
        );
    }

    /// <summary>
    /// Sends an order freeing the bound entities on the same channel, or optionally to just
    /// a single unit.
    /// </summary>
    /// <param name="holder">Entity holding the remote control.</param>
    /// <param name="pointed">The entity pointed to by the holder.</param>
    /// <param name="recipient">Optional recipient, if set then only that specific entity will receive the order.</param>
    /// <returns>True if the order was sent successfuly, false otherwise.</returns>
    public bool SendFreeUnitOrder(Entity<RemoteControlHolderComponent?> holder, EntityUid pointed, EntityUid? recipient = null)
    {
        return TrySendOrder(
            holder,
            RemoteControlOrderType.FreeUnit,
            new EntityCoordinates(pointed, Vector2.Zero),
            (control) => new RemoteControlFreeUnitOrderEvent(holder, control.Owner, control.Comp.BoundEntities),
            recipient
        );
    }

    /// <summary>
    /// Attempts to send an order to all available receivers, or a single recipient.
    /// </summary>
    /// <typeparam name="T">The type of event to send.</typeparam>
    /// <param name="holder">The entity currently holding/using the whistle.</param>
    /// <param name="getEvent">A function providing the event to send to users.</param>
    /// <param name="recipient">An optional recipient entity to receive this event.</param>
    /// <returns>True if the order was sent, otherwise false.</returns>
    private bool TrySendOrder<T>(
        Entity<RemoteControlHolderComponent?> holder,
        RemoteControlOrderType orderType,
        EntityCoordinates pointCoords,
        Func<Entity<RemoteControlComponent>, T> getEvent,
        EntityUid? recipient = null
    ) where T : notnull
    {
        if (!Resolve(holder, ref holder.Comp))
            return false;

        if (!TryComp<RemoteControlComponent>(holder.Comp.Control, out var controlComp))
            return false;

        Entity<RemoteControlComponent> control = (holder.Comp.Control, controlComp);
        var toggleAction = Actions.GetAction(CompOrNull<ToggleClothingComponent>(control.Owner)?.ActionEntity);
        if (!toggleAction.HasValue)
            return false;

        if (UseDelay.IsDelayed(control.Owner) ||
            toggleAction.HasValue && Actions.IsCooldownActive(toggleAction.Value.Comp))
            return false;

        UseDelay.TryResetDelay(control.Owner);
        Actions.SetCooldown(toggleAction?.AsNullable(), control.Comp.Cooldown);

        var ev = getEvent(control);
        if (recipient.HasValue)
        {
            if (!TryComp<RemoteControlReceiverComponent>(recipient, out var receiverComp) ||
                receiverComp.ChannelName != control.Comp.ChannelName)
                return false;

            RaiseLocalEvent(recipient.Value, ref ev);
        }
        else
        {
            var query = EntityQueryEnumerator<RemoteControlReceiverComponent>();
            while (query.MoveNext(out var ent, out var receiverComp))
            {
                if (receiverComp.ChannelName != control.Comp.ChannelName)
                    continue; // Not on the same channel, ignore

                RaiseLocalEvent(ent, ref ev);
            }
        }

        var message = Loc.GetString(control.Comp.OrderStrings.GetValueOrDefault(orderType, _defaultLocString));
        Popup.PopupCoordinates(message, pointCoords, holder, Shared.Popups.PopupType.Medium);

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
            SendSelfPointOrder(holder.AsNullable(), args.Pointed);
        else if (
            TryComp<RemoteControlReceiverComponent>(args.Pointed, out var reciever) &&
            reciever.BoundControl == holder.Comp.Control)
            SendFreeUnitOrder(holder.AsNullable(), args.Pointed);
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
}
