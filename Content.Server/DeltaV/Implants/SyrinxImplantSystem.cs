using Content.Server.Administration.Logs;
using Content.Server.Implants;
using Content.Server.Speech.Components;
using Content.Shared.Database;
using Content.Shared.DeltaV.Implants;
using Content.Shared.Implants;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Preferences;
using Content.Shared.Speech;
using Content.Shared.VoiceMask;
using Robust.Shared.Prototypes;

namespace Content.Server.DeltaV.Implants;

public sealed class SyrinxImplantSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SyrinxImplantComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<SyrinxImplantComponent, ImplantRemovedEvent>(OnRemoved);
        SubscribeLocalEvent<SyrinxImplantComponent, VoiceMaskChangeNameMessage>(OnChangeName);
        SubscribeLocalEvent<SyrinxImplantComponent, VoiceMaskChangeVerbMessage>(OnChangeVerb);
        SubscribeLocalEvent<SyrinxImplantSetNameEvent>(OpenUI);
    }

    private void OnImplanted(Entity<SyrinxImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (args.Implanted is not {} user)
            return;

        ent.Comp.Existing = HasComp<VoiceOverrideComponent>(user);

        var voice = EnsureComp<VoiceOverrideComponent>(user);
        voice.NameOverride = Name(user);
    }

    private void OnRemoved(Entity<SyrinxImplantComponent> ent, ref ImplantRemovedEvent args)
    {
        if (args.Implanted is not {} user)
            return;

        if (ent.Comp.Existing && TryComp<VoiceOverrideComponent>(user, out var voice))
            voice.NameOverride = Name(user);
        else
            RemComp<VoiceOverrideComponent>(user);
    }

    private void OnChangeVerb(Entity<SyrinxImplantComponent> ent, ref VoiceMaskChangeVerbMessage msg)
    {
        var user = msg.Actor;
        if (!TryComp<VoiceOverrideComponent>(user, out var voice))
            return;

        if (msg.Verb is {} id && !_proto.HasIndex<SpeechVerbPrototype>(id))
            return;

        voice.SpeechVerbOverride = msg.Verb;
        // verb is only important to metagamers so no need to log as opposed to name

        _popup.PopupEntity(Loc.GetString("voice-mask-popup-success"), ent, user);

        UpdateUI(ent, voice);
    }

    /// <summary>
    /// Copy from VoiceMaskSystem, adapted to work with SyrinxImplantComponent.
    /// </summary>
    private void OnChangeName(Entity<SyrinxImplantComponent> ent, ref VoiceMaskChangeNameMessage msg)
    {
        var user = msg.Actor;
        if (!TryComp<VoiceOverrideComponent>(user, out var voice))
            return;

        if (msg.Name.Length > HumanoidCharacterProfile.MaxNameLength || msg.Name.Length <= 0)
        {
            _popup.PopupEntity(Loc.GetString("voice-mask-popup-failure"), user, user, PopupType.SmallCaution);
            return;
        }

        voice.NameOverride = msg.Name.Trim();
        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(user):player} set voice of {ToPrettyString(ent):mask}: {voice.NameOverride}");

        _popup.PopupEntity(Loc.GetString("voice-mask-popup-success"), user, user);

        UpdateUI(ent, voice);
    }

    /// <summary>
    /// Copy from VoiceMaskSystem, adapted to work with SyrinxImplantComponent.
    /// </summary>
    private void UpdateUI(EntityUid uid, VoiceOverrideComponent voice)
    {
        var state = new VoiceMaskBuiState(voice.NameOverride ?? Loc.GetString("voice-mask-default-name-override"), voice.SpeechVerbOverride);
        _ui.SetUiState(uid, VoiceMaskUIKey.Key, state);
    }

    private void OpenUI(SyrinxImplantSetNameEvent args)
    {
        var user = args.Performer;
        if (!TryComp<VoiceOverrideComponent>(user, out var voice))
            return;

        var uid = args.Action.Comp.Container!.Value;
        _ui.TryOpenUi(uid, VoiceMaskUIKey.Key, user);
        UpdateUI(uid, voice);
    }
}
