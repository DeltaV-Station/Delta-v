// Most of this file is taken from Server class RadioDeviceSystem
using System.Linq;
using Content.Shared._DV.Radio.Components;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Radio.Components;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

// Namespaced to original SharedRadioDeviceSystem because it's a part
namespace Content.Shared.Radio.EntitySystems;

/// <summary>
/// This system handles radio speakers and microphones (which together form a hand-held radio).
/// </summary>
public abstract partial class SharedRadioDeviceSystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadioMicrophoneComponent, ComponentInit>(OnMicrophoneInit);
        SubscribeLocalEvent<RadioMicrophoneComponent, ExaminedEvent>(OnExamineMicrophone);
        SubscribeLocalEvent<RadioMicrophoneComponent, ActivateInWorldEvent>(OnActivateMicrophone);
        SubscribeLocalEvent<RadioMicrophoneComponent, ListenAttemptEvent>(OnAttemptListen);
        SubscribeLocalEvent<RadioMicrophoneComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<RadioMicrophoneComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbsMicrophone);

        SubscribeLocalEvent<RadioSpeakerComponent, ComponentInit>(OnSpeakerInit);
        SubscribeLocalEvent<RadioSpeakerComponent, ExaminedEvent>(OnExamineSpeaker);
        SubscribeLocalEvent<RadioSpeakerComponent, ActivateInWorldEvent>(OnActivateSpeaker);
        SubscribeLocalEvent<RadioSpeakerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbsSpeaker);

        SubscribeLocalEvent<IntercomComponent, EncryptionChannelsChangedEvent>(OnIntercomEncryptionChannelsChanged);
        SubscribeLocalEvent<IntercomComponent, ToggleIntercomMicMessage>(OnToggleIntercomMic);
        SubscribeLocalEvent<IntercomComponent, ToggleIntercomSpeakerMessage>(OnToggleIntercomSpeaker);
        SubscribeLocalEvent<IntercomComponent, SelectIntercomChannelMessage>(OnSelectIntercomChannel);
    }

    #region Component Init
    // Taken verbatim from Server
    private void OnMicrophoneInit(EntityUid uid, RadioMicrophoneComponent component, ComponentInit args)
    {
        if (component.Enabled)
            EnsureComp<ActiveListenerComponent>(uid).Range = component.ListenRange;
        else
            RemCompDeferred<ActiveListenerComponent>(uid);
    }

    // Taken verbatim from Server
    private void OnSpeakerInit(EntityUid uid, RadioSpeakerComponent component, ComponentInit args)
    {
        if (component.Enabled)
            EnsureComp<ActiveRadioComponent>(uid).Channels.UnionWith(component.Channels);
        else
            RemCompDeferred<ActiveRadioComponent>(uid);
    }
    #endregion

    #region Toggling
    // Taken verbatim from Server
    private void OnActivateMicrophone(EntityUid uid, RadioMicrophoneComponent component, ActivateInWorldEvent args)
    {
        if (!args.Complex)
            return;

        if (!component.ToggleOnInteract)
            return;

        ToggleRadioMicrophone(uid, args.User, args.Handled, component);
        args.Handled = true;
    }

    // Taken verbatim from Server
    private void OnActivateSpeaker(EntityUid uid, RadioSpeakerComponent component, ActivateInWorldEvent args)
    {
        if (!args.Complex)
            return;

        if (!component.ToggleOnInteract)
            return;

        ToggleRadioSpeaker(uid, args.User, args.Handled, component);
        args.Handled = true;
    }

    // Taken verbatim from Server
    private void OnPowerChanged(EntityUid uid, RadioMicrophoneComponent component, ref PowerChangedEvent args)
    {
        if (args.Powered)
            return;
        SetMicrophoneEnabled(uid, null, false, true, component);
    }

    // Taken mostly from Server
    // Was previously virtual
    private void SetMicrophoneEnabled(EntityUid uid, EntityUid? user, bool enabled, bool quiet = false, RadioMicrophoneComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        if (component.PowerRequired && !_power.IsPowered(uid))
            return;

        component.Enabled = enabled;
        Dirty(uid, component);

        if (!quiet && user != null)
        {
            var state = GetStatusText(component);
            var message = Loc.GetString("radio-microphone-component-on-use", ("radioState", state));
            _popup.PopupPredicted(message, uid, user.Value);
        }

        _appearance.SetData(uid, RadioDeviceVisuals.Broadcasting, component.Enabled);
        if (component.Enabled)
            EnsureComp<ActiveListenerComponent>(uid).Range = component.ListenRange;
        else
            RemCompDeferred<ActiveListenerComponent>(uid);
    }

    /// <summary>
    /// Sets the <see cref="RadioSpeakerComponent"/>'s and <see cref="ActiveRadioComponent"/>'s channels.
    /// </summary>
    /// <param name="entity">The radio whose channels are being set.</param>
    /// <param name="channels">The HashSet of channels to set.</param>
    private void SetSpeakerChannels(Entity<RadioSpeakerComponent> entity, HashSet<ProtoId<RadioChannelPrototype>> channels)
    {
        entity.Comp.Channels.Clear();
        entity.Comp.Channels.UnionWith(channels);
        UpdateActiveRadioComponent(entity);
    }
    #endregion

    #region Examine
    /// <summary>
    /// Examine event handler for microphone
    /// </summary>
    private void OnExamineMicrophone(EntityUid uid, RadioMicrophoneComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var proto = _protoMan.Index(component.BroadcastChannel);
        var status = GetStatusText(component, true);

        // Analogous to Encryption Keyholder examine.
        using (args.PushGroup(nameof(RadioMicrophoneComponent)))
        {
            args.PushMarkup(Loc.GetString("radio-microphone-component-on-examine",
                ("status", status),
                ("color", proto.Color),
                ("channel", proto.LocalizedName),
                ("frequency", proto.Frequency)));
        }
    }

    /// <summary>
    /// Examine event handler for speaker
    /// </summary>
    private void OnExamineSpeaker(EntityUid uid, RadioSpeakerComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var status = GetStatusText(component, true);

        // Analogous to Encryption Keyholder examine.
        using (args.PushGroup(nameof(RadioSpeakerComponent)))
        {
            args.PushMarkup(Loc.GetString("radio-speaker-component-on-examine", ("status", status)));

            foreach (var channel in component.Channels)
            {
                var proto = _protoMan.Index(channel);
                var color = proto.Color;
                var name = proto.LocalizedName;
                var frequency = proto.Frequency;

                args.PushMarkup(Loc.GetString("radio-speaker-component-examine-channel",
                    ("color", color),
                    ("channel", name),
                    ("frequency", frequency)));
            }
        }
    }

    /// <summary>
    /// Returns status of the component "on" or "off",
    /// optionally with color formatting.
    /// </summary>
    /// <param name="component">Component state of which is checked.</param>
    /// <param name="color">If true, will return color-formatted version of the string.</param>
    /// <returns></returns>
    private string GetStatusText(DVRadioToggleable component, bool color = false)
    {
        LocId locId = component.Enabled ? "radio-device-on-state" : "radio-device-off-state";
        if (color)
            locId += "-color";
        return Loc.GetString(locId);
    }
    #endregion

    #region Verbs
    /// <summary>
    /// Get Verbs even handler for microphone.
    /// Adds Enable/Disable microphone verb.
    /// </summary>
    private void OnGetVerbsMicrophone(EntityUid uid, RadioMicrophoneComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        GetVerbs(component, args, 3, () => SetMicrophoneEnabled(uid, args.User, !component.Enabled));
    }

    /// <summary>
    /// Get Verbs even handler for speaker.
    /// Adds Enable/Disable speaker verb.
    /// </summary>
    private void OnGetVerbsSpeaker(EntityUid uid, RadioSpeakerComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        GetVerbs(component, args, 4, () => SetSpeakerEnabled(uid, args.User, !component.Enabled));
    }

    /// <summary>
    /// Adds Enable/Disable verb to <paramref name="args"/>, using the <paramref name="component"/>'s verb-text locid fields.
    /// </summary>
    /// <param name="component">Component is a DVRadioToggleable, so basically RadioMicrophone or RadioSpeaker component</param>
    /// <param name="args"></param>
    /// <param name="priority">Verb's priority</param>
    /// <param name="action">Action to be executed on verb activation</param>
    private void GetVerbs(DVRadioToggleable component, GetVerbsEvent<AlternativeVerb> args, int priority, Action action)
    {
        if (args.Hands == null || !args.CanAccess || !args.CanInteract || !component.Toggleable)
            return;

        var verbText = component.Enabled ? component.DisableVerbText : component.EnableVerbText;

        AlternativeVerb verb = new()
            {
                Text = Loc.GetString(verbText),
                Act = action,
                Priority = priority
            };

        args.Verbs.Add(verb);
    }
    #endregion

    // Taken pretty much verbatim from Server
    private void OnAttemptListen(EntityUid uid, RadioMicrophoneComponent component, ListenAttemptEvent args)
    {
        if (component.PowerRequired && !_power.IsPowered(uid)
            || component.UnobstructedRequired && !_interaction.InRangeUnobstructed(args.Source, uid, 0))
        {
            args.Cancel();
        }
    }

    #region Intercom
    // Taken from Server with modifications
    /// <summary>
    /// Handles Intercom's encryption keys change.
    /// Assumes RadioMicrophone/RadioSpeaker's channel to be the one selected.
    /// </summary>
    private void OnIntercomEncryptionChannelsChanged(Entity<IntercomComponent> ent, ref EncryptionChannelsChangedEvent args)
    {
        ent.Comp.SupportedChannels = args.Component.Channels.Select(p => new ProtoId<RadioChannelPrototype>(p)).ToList();
        Dirty(ent);

        // Sets channel to Microphone component's current channel if its key is inserted.
        // Falling back to Speaker component's current channel if its key is inserted.
        // Falling back to default channel.
        var channel = args.Component.DefaultChannel;
        if (TryComp(ent, out RadioMicrophoneComponent? microphone))
        {
            if (ent.Comp.SupportedChannels.Contains(microphone.BroadcastChannel))
                channel = microphone.BroadcastChannel;
        }
        else if (TryComp(ent, out RadioSpeakerComponent? speaker))
        {
            var speakerChannel = speaker.Channels.First();
            if (ent.Comp.SupportedChannels.Contains(speakerChannel))
                channel = speakerChannel;
        }

        SetIntercomChannel(ent, channel);
    }

    // Taken from Server with modifications
    /// <summary>
    /// Handles Intercom UI's microphone toggle,
    /// setting RadioMicrophone's Enabled field.
    /// </summary>
    private void OnToggleIntercomMic(Entity<IntercomComponent> ent, ref ToggleIntercomMicMessage args)
    {
        if (ent.Comp.RequiresPower && !_power.IsPowered(ent.Owner))
            return;

        SetMicrophoneEnabled(ent, args.Actor, args.Enabled, true);
    }

    // Taken from Server with modifications
    /// <summary>
    /// Handles Intercom UI's speaker toggle.
    /// /// setting RadioSpeaker's Enabled field.
    /// </summary>
    private void OnToggleIntercomSpeaker(Entity<IntercomComponent> ent, ref ToggleIntercomSpeakerMessage args)
    {
        if (ent.Comp.RequiresPower && !_power.IsPowered(ent.Owner))
            return;

        SetSpeakerEnabled(ent, args.Actor, args.Enabled, true);
    }

    // Taken from Server with modifications
    /// <summary>
    /// Handles Intercom UI's channel selector,
    /// setting channel on both RadioMicrophone and RadioSpeaker.
    /// </summary>
    private void OnSelectIntercomChannel(Entity<IntercomComponent> ent, ref SelectIntercomChannelMessage args)
    {
        if (ent.Comp.RequiresPower && !_power.IsPowered(ent.Owner))
            return;

        if (!_protoMan.HasIndex<RadioChannelPrototype>(args.Channel) || !ent.Comp.SupportedChannels.Contains(args.Channel))
            return;

        SetIntercomChannel(ent, args.Channel);
    }

    // Taken from Server with modifications
    /// <summary>
    /// Sets channel on both RadioMicrophone and RadioSpeaker.
    /// Disables both if channel is null.
    /// </summary>
    private void SetIntercomChannel(Entity<IntercomComponent> ent, ProtoId<RadioChannelPrototype>? channel)
    {
        if (channel == null)
        {
            SetSpeakerEnabled(ent, null, false);
            SetMicrophoneEnabled(ent, null, false);
            return;
        }

        if (TryComp<RadioMicrophoneComponent>(ent, out var microphone))
        {
            microphone.BroadcastChannel = channel.Value;
            Dirty(ent.Owner, microphone);
        }

        if (TryComp<RadioSpeakerComponent>(ent, out var speaker))
        {
            SetSpeakerChannels((ent, speaker), [ channel.Value ]);
            Dirty(ent.Owner, speaker);
        }
    }
    #endregion
}
