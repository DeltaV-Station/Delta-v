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
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly RadioSystem _radioSystem = default!;

    private EntityQuery<ActorComponent> _actorQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadioImplantComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RadioImplantComponent, EntInsertedIntoContainerMessage>(OnInsertEncryptionKey);
        SubscribeLocalEvent<RadioImplantComponent, EntRemovedFromContainerMessage>(OnRemoveEncryptionKey);
        SubscribeLocalEvent<RadioImplantComponent, RadioReceiveEvent>(OnRadioReceive);
        SubscribeLocalEvent<HasRadioImplantComponent, EntitySpokeEvent>(OnSpeak);
        _actorQuery = GetEntityQuery<ActorComponent>();
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
        if (args.Channel is null)
            return;

        // does the implant have access to the channel the implantee is trying to speak on?
        if (ent.Comp.Implant is {} implant
            && TryComp<RadioImplantComponent>(implant, out var radioImplantComponent)
            && radioImplantComponent.Channels.Contains(args.Channel.ID))
        {
            _radioSystem.SendRadioMessage(ent, args.Message, args.Channel.ID, implant);
            // prevent other radios they might be wearing from sending the message again
            args.Channel = null;
        }
    }

    /// <summary>
    /// Handles receiving radio messages and forwarding them to the implantee.
    /// </summary>
    private void OnRadioReceive(EntityUid uid, RadioImplantComponent component, ref RadioReceiveEvent args)
    {
        if (_actorQuery.TryComp(component.Implantee, out var actorComponent))
            _netManager.ServerSendMessage(args.ChatMsg, actorComponent.PlayerSession.Channel);
    }

    /// <summary>
    /// Handles the addition of an encryption key to the implant's storage.
    /// </summary>
    private void OnInsertEncryptionKey(Entity<RadioImplantComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        // check if the insertion is actually something getting inserted into the radio implant storage, since
        // this evt also fires when the radio implant is being inserted into a person.
        if (ent.Owner != args.Container.Owner
            || !TryComp<EncryptionKeyComponent>(args.Entity, out var encryptionKeyComponent))
            return;

        // copy over the radio channels that can be accessed
        ent.Comp.Channels.Clear();
        foreach (var channel in encryptionKeyComponent.Channels)
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
        ent.Comp.Channels.Clear();
        Dirty(ent);
        UpdateRadioReception(ent);
    }

    /// <summary>
    /// Ensures that this thing can actually hear radio messages from channels the key provides.
    /// </summary>
    private void UpdateRadioReception(Entity<RadioImplantComponent> ent)
    {
        if (ent.Comp.Channels.Count != 0)
        {
            // we need to add this comp to actually receive radio events.
            var channels = EnsureComp<ActiveRadioComponent>(ent).Channels;
            foreach (var channel in ent.Comp.Channels)
            {
                channels.Add(channel);
            }
        }
        else
        {
            RemComp<ActiveRadioComponent>(ent);
        }
    }
}
