using System.Numerics;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Shared._DV.DogWhistle.Components;
using Content.Shared._DV.DogWhistle.EntitySystems;
using Content.Shared._DV.DogWhistle.Events;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._DV.DogWhistle.EntitySystems;

/// <summary>
/// Server side handling of Dog Whistles.
/// Specifically handling recieving orders.
/// </summary>
public sealed partial class DogWhistleSystem : SharedDogWhistleSystem
{
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    private readonly string _targetKey = "TargetCoordinates";

    /// <summary>
    /// Handles when a catch order is sent via a whistle.
    /// </summary>
    /// <param name="ent">Entity recieving the order.</param>
    /// <param name="args">Args for the event, notably sound and origin.</param>
    private void OnCatchOrder(Entity<DogWhistleRecieverComponent> ent, ref DogWhistleCatchOrderEvent args)
    {
        if (IsNPC(ent))
        {
            if (!CanNPCHandleOrder(ent, args.BoundNPC))
                return;

            var target = args.Target;
            UpdateNPCOrders(ent,
                DogWhistleOrderType.Catch,
                () =>
                {
                    _npcSystem.SetBlackboard(ent, NPCBlackboard.CurrentOrderedTarget, target);
                    _npcSystem.SetBlackboard(ent, _targetKey, new EntityCoordinates(target, Vector2.Zero));
                    return true;
                });
        }
        else
        {
            HandleOrderEffects(ent, args.Sound, args.Origin, ent.Comp.CatchOrder);
        }
    }

    /// <summary>
    /// Handles when a sit order is sent via a whistle.
    /// </summary>
    /// <param name="ent">Entity recieving the order.</param>
    /// <param name="args">Args for the event, notably sound and origin.</param>
    private void OnSitOrder(Entity<DogWhistleRecieverComponent> ent, ref DogWhistleSitOrderEvent args)
    {
        if (IsNPC(ent))
        {
            if (!CanNPCHandleOrder(ent, args.BoundNPC))
                return;

            var location = _transformSystem.ToCoordinates(args.Origin, args.Location);
            UpdateNPCOrders(ent,
                DogWhistleOrderType.Sit,
                () =>
                {
                    _npcSystem.SetBlackboard(ent, NPCBlackboard.MovementTarget, location);
                    return true;
                });
        }
        else
        {
            HandleOrderEffects(ent, args.Sound, args.Origin, ent.Comp.SitOrder);
        }
    }

    /// <summary>
    /// Handles when a come back order is sent via a whistle.
    /// </summary>
    /// <param name="ent">Entity recieving the order.</param>
    /// <param name="args">Args for the event, notably sound and origin.</param>
    private void OnComebackOrder(Entity<DogWhistleRecieverComponent> ent, ref DogWhistleComebackOrderEvent args)
    {
        if (IsNPC(ent))
        {
            if (!CanNPCHandleOrder(ent, args.BoundNPC))
                return;

            var origin = args.Origin;
            UpdateNPCOrders(ent,
                DogWhistleOrderType.Comeback,
                () =>
                {
                    _npcSystem.SetBlackboard(ent,
                        NPCBlackboard.FollowTarget,
                        new EntityCoordinates(origin, Vector2.Zero));
                    return true;
                });
        }
        else
        {
            HandleOrderEffects(ent, args.Sound, args.Origin, ent.Comp.ComebackOrder);
        }
    }

    /// <summary>
    /// Checks whether the entity is an NPC or a player.
    /// </summary>
    /// <param name="ent">Entity to check for NPC status.</param>
    /// <returns>True if they are an NPC, false otherwise.</returns>
    private bool IsNPC(EntityUid ent)
    {
        return !_mindSystem.TryGetMind(ent, out _, out _);
    }

    /// <summary>
    /// Checks whether this entity can understand this NPC order, either by understanding or
    /// whether this order is meant for a specific entity.
    /// </summary>
    /// <param name="ent">NPC entity to check for.</param>
    /// <param name="boundNPC">Entity this order is bound to.</param>
    /// <returns>True if the NPC can understand the order, false otherwise.</returns>
    private bool CanNPCHandleOrder(Entity<DogWhistleRecieverComponent> ent, EntityUid? boundNPC)
    {
        if (!ent.Comp.CanUnderstand)
            return false; // Cannot understand this order, even as an NPC

        if (boundNPC.HasValue && ent.Owner != boundNPC)
            return false; // This order is not for this NPC

        return true;
    }

    /// <summary>
    /// Updates the NPCs current orders and runs the supplied action to populate any blackboard
    /// details that are required.
    /// </summary>
    /// <param name="npc">NPC to update orders for.</param>
    /// <param name="order">The current order enum they should follow.</param>
    /// <param name="orderAction">A lambda that updates any blackboard information for the current order.</param>
    private void UpdateNPCOrders(EntityUid npc, DogWhistleOrderType order, Func<bool> orderAction)
    {
        _npcSystem.SetBlackboard(npc, NPCBlackboard.CurrentOrders, order);

        if (!TryComp<HTNComponent>(npc, out var htn))
            return;

        if (htn.Plan != null)
            _htnSystem.ShutdownPlan(htn);

        orderAction();
        _htnSystem.Replan(htn);
    }

    /// <summary>
    /// Handles producing any general audio or visual effects the order has.
    /// Schedules sounds to be played and any popups that might need to be sent.
    /// </summary>
    /// <param name="ent">Entity recieving this order.</param>
    /// <param name="sound">The sound to play.</param>
    /// <param name="origin">The origin of the whistle that sent this order.</param>
    /// <param name="orderString">The localisation string to use for this order.</param>
    private void HandleOrderEffects(Entity<DogWhistleRecieverComponent> ent,
        SoundSpecifier sound,
        EntityUid origin,
        LocId orderString)
    {
        var audioParams = new AudioParams
        {
            MaxDistance = 300f, // Long distance, TODO: Configurable?
            RolloffFactor = 0f // Hearable at the same volume across the station
        };

        string message;
        if (ent.Comp.CanUnderstand)
        {
            message = Loc.GetString(orderString);
        }
        else
        {
            audioParams.Volume =
                -10f; // Lower the volume by a bunch so we don't overwhelm players that don't need to understand this
            message = Loc.GetString(_random.Pick(ent.Comp.Screeches));
        }

        _audioSystem.PlayEntity(_audioSystem.ResolveSound(sound), ent, origin, audioParams);
        PopupSystem.PopupEntity(message, ent, ent, PopupType.Medium);
    }
}
