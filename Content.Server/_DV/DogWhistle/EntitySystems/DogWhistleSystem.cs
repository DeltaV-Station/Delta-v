using Content.Shared._DV.DogWhistle.Components;
using Content.Shared._DV.DogWhistle.EntitySystems;
using Content.Shared._DV.DogWhistle.Events;
using Content.Shared.Pointing;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Random;

namespace Content.Server._DV.DogWhistle.EntitySystems;

/// <summary>
/// Server side handling of Dog Whistles.
/// </summary>
public sealed class DogWhistleSystem : SharedDogWhistleSystem
{
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly UseDelaySystem _useDelaySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DogWhistleHolderComponent, AfterPointedAtEvent>(OnPointedAtEntity);
        SubscribeLocalEvent<DogWhistleHolderComponent, AfterPointedAtTileEvent>(OnPointedAtTile);

        SubscribeLocalEvent<DogWhistleRecieverComponent, DogWhistleCatchOrderEvent>(OnCatchOrder);
        SubscribeLocalEvent<DogWhistleRecieverComponent, DogWhistleSitOrderEvent>(OnSitOrder);
        SubscribeLocalEvent<DogWhistleRecieverComponent, DogWhistleComebackOrderEvent>(OnComebackOrder);
    }

    /// <summary>
    /// Handles when an entity using a dog whistle points at an entity.
    /// </summary>
    /// <param name="holder">Entity holding/wearing the dog whistle.</param>
    /// <param name="args">Args for the event, notably the entity pointed at.</param>
    private void OnPointedAtEntity(Entity<DogWhistleHolderComponent> holder, ref AfterPointedAtEvent args)
    {
        if (!TryComp<DogWhistleComponent>(holder.Comp.Whistle, out var whistleComp))
            return;

        var whistle = (holder.Comp.Whistle, whistleComp);
        if (!CanSendOrder(whistle))
            return;

        if (holder.Owner == args.Pointed)
        {
            var ev = new DogWhistleComebackOrderEvent(holder, whistleComp.Sound);
            SendOrderToRecievers(whistle, ref ev);
        }
        else
        {
            var ev = new DogWhistleCatchOrderEvent(holder, whistleComp.Sound, args.Pointed);
            SendOrderToRecievers(whistle, ref ev);
        }
    }

    /// <summary>
    /// Handles when an entity using a dog whistle points at a tile.
    /// </summary>
    /// <param name="holder">Entity holding/wearing the dog whistle.</param>
    /// <param name="args">Args for the event, notably the tile pointed at.</param>
    private void OnPointedAtTile(Entity<DogWhistleHolderComponent> holder, ref AfterPointedAtTileEvent args)
    {
        if (!TryComp<DogWhistleComponent>(holder.Comp.Whistle, out var whistleComp))
            return;

        var whistle = (holder.Comp.Whistle, whistleComp);
        if (!CanSendOrder(whistle))
            return;

        var ev = new DogWhistleSitOrderEvent(holder, whistleComp.Sound, args.Pointed);
        SendOrderToRecievers(whistle, ref ev);
    }

    /// <summary>
    /// Sends an order from the whistle to all possible recievers.
    /// </summary>
    /// <typeparam name="T">Type of the order event to send.</typeparam>
    /// <param name="whistle">Whistle sending this order.</param>
    /// <param name="ev">Order event being sent.</param>
    private void SendOrderToRecievers<T>(Entity<DogWhistleComponent> whistle, ref T ev) where T : notnull
    {
        var query = EntityQueryEnumerator<DogWhistleRecieverComponent>();
        while (query.MoveNext(out var ent, out var _))
        {
            RaiseLocalEvent(ent, ref ev);
        }

        _useDelaySystem.TryResetDelay(whistle.Owner);
    }

    /// <summary>
    /// Checks whether an order can be sent by this whistle, used to limited
    /// the amount of spam one can send to whistle recievers.
    /// </summary>
    /// <param name="whistle">Whistle being used.</param>
    /// <returns>True if there is no use delay (cooldown) active for this whistle, false otherwise.</returns>
    private bool CanSendOrder(Entity<DogWhistleComponent> whistle)
    {
        return !_useDelaySystem.IsDelayed(whistle.Owner);
    }

    /// <summary>
    /// Handles when a catch order is sent via a whistle.
    /// </summary>
    /// <param name="ent">Entity recieving the order.</param>
    /// <param name="args">Args for the event, notably sound and origin.</param>
    private void OnCatchOrder(Entity<DogWhistleRecieverComponent> ent, ref DogWhistleCatchOrderEvent args)
    {
        HandleOrder(ent, args.Sound, args.Origin, ent.Comp.CatchOrder);
    }

    /// <summary>
    /// Handles when a sit order is sent via a whistle.
    /// </summary>
    /// <param name="ent">Entity recieving the order.</param>
    /// <param name="args">Args for the event, notably sound and origin.</param>
    private void OnSitOrder(Entity<DogWhistleRecieverComponent> ent, ref DogWhistleSitOrderEvent args)
    {
        HandleOrder(ent, args.Sound, args.Origin, ent.Comp.SitOrder);
    }

    /// <summary>
    /// Handles when a come back order is sent via a whistle.
    /// </summary>
    /// <param name="ent">Entity recieving the order.</param>
    /// <param name="args">Args for the event, notably sound and origin.</param>
    private void OnComebackOrder(Entity<DogWhistleRecieverComponent> ent, ref DogWhistleComebackOrderEvent args)
    {
        HandleOrder(ent, args.Sound, args.Origin, ent.Comp.ComebackOrder);
    }

    /// <summary>
    /// Actually handles the order that's been sent.
    /// Schedules sounds to be played and any popups that might need to be sent.
    /// </summary>
    /// <param name="ent">Entity recieving this order.</param>
    /// <param name="sound">The sound to play.</param>
    /// <param name="origin">The origin of the whistle that sent this order.</param>
    /// <param name="orderString">The localisation string to use for this order.</param>
    private void HandleOrder(Entity<DogWhistleRecieverComponent> ent,
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
