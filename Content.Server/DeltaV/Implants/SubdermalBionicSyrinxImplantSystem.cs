using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Server.VoiceMask;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Popups;
using Content.Shared.Preferences;
using Content.Shared.VoiceMask;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server.Implants;

public sealed class SubdermalBionicSyrinxImplantSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;

    [ValidatePrototypeId<SpeciesPrototype>]
    public const string HarpySpeciesId = "Harpy";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoiceMaskerComponent, ImplantImplantedEvent>(OnInsert);
        SubscribeLocalEvent<SyrinxVoiceMaskComponent, TransformSpeakerNameEvent>(OnSpeakerNameTransform);
        SubscribeLocalEvent<SyrinxVoiceMaskComponent, VoiceMaskChangeNameMessage>(OnChangeName);
        // We need to remove the SyrinxVoiceMaskComponent from the owner before the implant
        // is removed, so we need to execute before the SubdermalImplantSystem.
        SubscribeLocalEvent<VoiceMaskerComponent, EntGotRemovedFromContainerMessage>(OnRemove, before: new[] { typeof(SubdermalImplantSystem) });
    }

    private void OnInsert(EntityUid uid, VoiceMaskerComponent component, ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue)
            return;

        ApplyVoiceMask(args.Implanted.Value);
    }

    private void ApplyVoiceMask(EntityUid uid)
    {
        if (!TryComp<HumanoidAppearanceComponent>(uid, out var appearance))
            return;

        var voicemask = EnsureComp<SyrinxVoiceMaskComponent>(uid);
        voicemask.VoiceName = MetaData(uid).EntityName;

        if (_prototypeManager.Index(appearance.Species)?.ID != HarpySpeciesId)
        {
            voicemask.Enabled = false;
            _popupSystem.PopupEntity(Loc.GetString("syrinx-popup-implant-failure"), uid, uid, PopupType.MediumCaution);
        }

        Dirty(uid, voicemask);
    }

    private void OnRemove(EntityUid uid, VoiceMaskerComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (!TryComp<SubdermalImplantComponent>(uid, out var implanted) || implanted.ImplantedEntity == null)
            return;

        RemComp<SyrinxVoiceMaskComponent>((EntityUid) implanted.ImplantedEntity);
    }

    /// <summary>
    /// Copy from VoiceMaskSystem, adapted to work with SyrinxVoiceMaskComponent.
    /// </summary>
    private void OnChangeName(EntityUid uid, SyrinxVoiceMaskComponent component, VoiceMaskChangeNameMessage message)
    {
        if (message.Name.Length > HumanoidCharacterProfile.MaxNameLength || message.Name.Length <= 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("voice-mask-popup-failure"), uid, uid, PopupType.MediumCaution);
            return;
        }

        if (TryComp<HumanoidAppearanceComponent>(uid, out var appearance) &&
            _prototypeManager.Index(appearance.Species)?.ID != HarpySpeciesId)
        {
            _popupSystem.PopupEntity(Loc.GetString("syrinx-popup-name-failure"), uid, uid, PopupType.MediumCaution);
            return;
        }

        component.VoiceName = message.Name;
        if (message.Session.AttachedEntity != null)
            _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(message.Session.AttachedEntity.Value):player} set voice of {ToPrettyString(uid):mask}: {component.VoiceName}");
        else
            _adminLogger.Add(LogType.Action, LogImpact.Medium, $"Voice of {ToPrettyString(uid):mask} set: {component.VoiceName}");

        _popupSystem.PopupCursor(Loc.GetString("voice-mask-popup-success"), message.Session);
        TrySetLastKnownName(uid, message.Name);
        UpdateUI(uid, component);
    }

    /// <summary>
    /// Copy from VoiceMaskSystem, adapted to work with SyrinxVoiceMaskComponent.
    /// </summary>
    private void TrySetLastKnownName(EntityUid implanted, string lastName)
    {
        if (!HasComp<VoiceMaskComponent>(implanted)
            || !TryComp<VoiceMaskerComponent>(implanted, out var maskComp))
            return;

        maskComp.LastSetName = lastName;
    }

    /// <summary>
    /// Copy from VoiceMaskSystem, adapted to work with SyrinxVoiceMaskComponent.
    /// </summary>
    private void UpdateUI(EntityUid owner, SyrinxVoiceMaskComponent? component = null)
    {
        if (!Resolve(owner, ref component, logMissing: false))
            return;

        if (_uiSystem.TryGetUi(owner, VoiceMaskUIKey.Key, out var bui))
            _uiSystem.SetUiState(bui, new VoiceMaskBuiState(component.VoiceName));
    }

    /// <summary>
    /// Copy from VoiceMaskSystem, adapted to work with SyrinxVoiceMaskComponent.
    /// </summary>
    private void OnSpeakerNameTransform(EntityUid uid, SyrinxVoiceMaskComponent component, TransformSpeakerNameEvent args)
    {
        if (component.Enabled)
            args.Name = component.VoiceName;
    }
}
