using Content.Server.Chat.Systems;
using Content.Server.Radio;
using Content.Server.Radio.Components;
using Content.Server.Radio.EntitySystems;
using Content.Shared.DeltaV.Implants.Radio;
using Content.Shared.Radio.Components;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.DeltaV.Implants.Radio;

public sealed class RadioImplantSystem : SharedRadioImplantSystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly RadioSystem _radioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadioImplantComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RadioImplantComponent, EntInsertedIntoContainerMessage>(OnInsertEncryptionKey);
        SubscribeLocalEvent<RadioImplantComponent, EntRemovedFromContainerMessage>(OnRemoveEncryptionKey);
        SubscribeLocalEvent<RadioImplantComponent, RadioReceiveEvent>(OnRadioReceive);
        SubscribeLocalEvent<HasRadioImplantComponent, EntitySpokeEvent>(OnSpeak);
    }

    /// <summary>
    /// Ensures implants with fixed channels work.
    /// </summary>
    private void OnMapInit(EntityUid uid, RadioImplantComponent component, MapInitEvent args)
    {
        UpdateRadioReception(uid, component);
    }

    /// <summary>
    /// Handles the implantee's speech being forwarded onto the radio channel of the implant.
    /// </summary>
    private void OnSpeak(EntityUid uid, HasRadioImplantComponent hasRadioImplantComponent, EntitySpokeEvent args)
    {
        // not a radio message, or already handled by another radio
        if (args.Channel is null)
            return;

        // does the implant have access to the channel the implantee is trying to speak on?
        if (hasRadioImplantComponent.Implant is { Valid: true }
            && TryComp<RadioImplantComponent>(hasRadioImplantComponent.Implant, out var radioImplantComponent)
            && radioImplantComponent.Channels.Contains(args.Channel.ID))
        {
            _radioSystem.SendRadioMessage(uid, args.Message, args.Channel.ID, hasRadioImplantComponent.Implant.Value);
            // prevent other radios they might be wearing from sending the message again
            args.Channel = null;
        }
    }

    /// <summary>
    /// Handles receiving radio messages and forwarding them to the implantee.
    /// </summary>
    private void OnRadioReceive(EntityUid uid, RadioImplantComponent component, ref RadioReceiveEvent args)
    {
        if (TryComp(component.Implantee, out ActorComponent? actorComponent))
            _netManager.ServerSendMessage(args.ChatMsg, actorComponent.PlayerSession.Channel);
    }

    /// <summary>
    /// Handles the addition of an encryption key to the implant's storage.
    /// </summary>
    private void OnInsertEncryptionKey(EntityUid uid, RadioImplantComponent component, EntInsertedIntoContainerMessage args)
    {
        // check if the insertion is actually something getting inserted into the radio implant storage, since
        // this evt also fires when the radio implant is being inserted into a person.
        if (uid != args.Container.Owner
            || !TryComp<EncryptionKeyComponent>(args.Entity, out var encryptionKeyComponent))
            return;

        // copy over the radio channels that can be accessed
        component.Channels.Clear();
        component.Channels.UnionWith(encryptionKeyComponent.Channels);
        Dirty(uid, component);
        UpdateRadioReception(uid, component);
    }

    /// <summary>
    /// Handles the removal of an encryption key from the implant's storage.
    /// </summary>
    private void OnRemoveEncryptionKey(EntityUid uid, RadioImplantComponent component, EntRemovedFromContainerMessage args)
    {
        // check if the insertion is actually something getting inserted into the radio implant storage, since
        // this evt also fires when the radio implant is being inserted into a person.
        if (uid != args.Container.Owner
            || !HasComp<EncryptionKeyComponent>(args.Entity))
            return;

        // clear the radio channels since there's no encryption key inserted anymore.
        component.Channels.Clear();
        Dirty(uid, component);
        UpdateRadioReception(uid, component);
    }

    /// <summary>
    /// Ensures that this thing can actually hear radio messages from channels the key provides.
    /// </summary>
    private void UpdateRadioReception(EntityUid uid, RadioImplantComponent component)
    {
        if (component.Channels.Count != 0)
        {
            // we need to add this comp to actually receive radio events.
            EnsureComp<ActiveRadioComponent>(uid).Channels = new(component.Channels);
        }
        else
        {
            RemComp<ActiveRadioComponent>(uid);
        }
    }
}
