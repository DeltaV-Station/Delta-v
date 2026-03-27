using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Popups;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._DV.MetalDetector;

/// <summary>
/// WIP
/// </summary>
public sealed class MetalDetectorSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetalDetectorComponent, StepTriggeredOnEvent>(HandleStepOnTriggered);
        SubscribeLocalEvent<MetalDetectorComponent, StepTriggeredOffEvent>(HandleStepOffTriggered);
        SubscribeLocalEvent<MetalDetectorComponent, StepTriggerAttemptEvent>(HandleStepTriggerAttempt);
    }

    private void HandleStepOnTriggered(EntityUid uid, MetalDetectorComponent component, ref StepTriggeredOnEvent args)
    {
        LocId triggerText = "land-mine-triggered";
        _popupSystem.PopupClient(
            Loc.GetString(triggerText, ("mine", uid)),
            Transform(uid).Coordinates,
            args.Tripper,
            PopupType.LargeCaution);
        SoundSpecifier sound = new SoundPathSpecifier("/Audio/Effects/beep_landmine.ogg");
        _audioSystem.PlayLocal(sound, uid, uid, null);
    }

    private void HandleStepOffTriggered(EntityUid uid, MetalDetectorComponent component, ref StepTriggeredOffEvent args)
    {
    }

    private void HandleStepTriggerAttempt(EntityUid uid,
        MetalDetectorComponent component,
        ref StepTriggerAttemptEvent args)
    {
        if (TryComp<ItemToggleComponent>(uid, out var toggle))
        {
            args.Continue = toggle.Activated;
        }
    }
}
