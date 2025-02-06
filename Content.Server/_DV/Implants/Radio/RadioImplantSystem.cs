using Content.Server.Chat.Systems;
using Content.Server.Radio;
using Content.Server.Radio.Components;
using Content.Server.Radio.EntitySystems;
using Content.Shared._DV.Implants.Radio;
using Content.Shared.Radio.Components;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._DV.Implants.Radio;

/// <inheritdoc />
public sealed class RadioImplantSystem : SharedRadioImplantSystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly RadioSystem _radio = default!;

    private EntityQuery<ActorComponent> _actor;

    public override void Initialize()
    {
        base.Initialize();

        _actor = GetEntityQuery<ActorComponent>();

        SubscribeLocalEvent<RadioImplantComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RadioImplantComponent, EntInsertedIntoContainerMessage>(OnInsertEncryptionKey);
        SubscribeLocalEvent<RadioImplantComponent, EntRemovedFromContainerMessage>(OnRemoveEncryptionKey);
        SubscribeLocalEvent<RadioImplantComponent, RadioReceiveEvent>(OnRadioReceive);
        SubscribeLocalEvent<HasRadioImplantComponent, EntitySpokeEvent>(OnSpeak);
    }

    /// <summary>
    /// Ensures implants with fixed channels work.
    /// </summary>
    private void OnMapInit(Entity<RadioImplantComponent> ent, ref MapInitEvent args)
    {
        UpdateRadioReception(ent);
    }

    /// <summary>
    /// Handles the implantee's speech being forwarded onto the radio channel of the implant.
    /// </summary>
    private void OnSpeak(Entity<HasRadioImplantComponent> ent, ref EntitySpokeEvent args)
    {
        // not a radio message, or already handled by another radio
        if (args.Channel is not {} channel)
            return;

        // does an implant have access to the channel the implantee is trying to speak on?
        foreach (var implant in ent.Comp.Implants)
        {
            if (TryComp<RadioImplantComponent>(implant, out var radioImplant) &&
                radioImplant.Channels.Contains(channel.ID))
            {
                _radio.SendRadioMessage(ent, args.Message, channel.ID, implant);
                // prevent other radios they might be wearing from sending the message again
                args.Channel = null;
            }
        }
    }

    /// <summary>
    /// Handles receiving radio messages and forwarding them to the implantee.
    /// </summary>
    private void OnRadioReceive(Entity<RadioImplantComponent> ent, ref RadioReceiveEvent args)
    {
        if (_actor.TryComp(ent.Comp.Implantee, out var actor))
            _net.ServerSendMessage(args.ChatMsg, actor.PlayerSession.Channel);
    }

    /// <summary>
    /// Handles the addition of an encryption key to the implant's storage.
    /// </summary>
    private void OnInsertEncryptionKey(Entity<RadioImplantComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        // check if the insertion is actually something getting inserted into the radio implant storage, since
        // this evt also fires when the radio implant is being inserted into a person.
        if (ent.Owner != args.Container.Owner
            || !TryComp<EncryptionKeyComponent>(args.Entity, out var key))
            return;

        // copy over the radio channels that can be accessed
        ent.Comp.Channels.Clear();
        foreach (var channel in key.Channels)
        {
            ent.Comp.Channels.Add(channel);
        }
        Dirty(ent);
        UpdateRadioReception(ent);
    }

    /// <summary>
    /// Handles the removal of an encryption key from the implant's storage.
    /// </summary>
    private void OnRemoveEncryptionKey(Entity<RadioImplantComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        // check if the insertion is actually something getting inserted into the radio implant storage, since
        // this evt also fires when the radio implant is being inserted into a person.
        if (ent.Owner != args.Container.Owner
            || !HasComp<EncryptionKeyComponent>(args.Entity))
            return;

        // clear the radio channels since there's no encryption key inserted anymore.
        // if you ever make the storage have more than 1 key's space you will have to rebuild it instead
        ent.Comp.Channels.Clear();
        Dirty(ent);
        UpdateRadioReception(ent);
    }

    /// <summary>
    /// Ensures that this thing can actually hear radio messages from channels the key provides.
    /// </summary>
    private void UpdateRadioReception(Entity<RadioImplantComponent> ent)
    {
        if (ent.Comp.Channels.Count == 0)
        {
            RemComp<ActiveRadioComponent>(ent);
            return;
        }

        // we need to add this comp to actually receive radio events.
        var channels = EnsureComp<ActiveRadioComponent>(ent).Channels;
        foreach (var channel in ent.Comp.Channels)
        {
            channels.Add(channel);
        }
    }
}
