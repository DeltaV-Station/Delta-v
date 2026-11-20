using Content.Shared.Popups;
using Content.Shared.Radio.Components;

namespace Content.Shared.Radio.EntitySystems;

// DeltaV - Made partial. Added a part to this class in _DV
public abstract partial class SharedRadioDeviceSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    #region Toggling
    public void ToggleRadioMicrophone(EntityUid uid, EntityUid user, bool quiet = false, RadioMicrophoneComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        SetMicrophoneEnabled(uid, user, !component.Enabled, quiet, component);
    }

    // DeltaV - Implemented in the class part inside _DV
    // public virtual void SetMicrophoneEnabled(EntityUid uid, EntityUid? user, bool enabled, bool quiet = false, RadioMicrophoneComponent? component = null) { }

    public void ToggleRadioSpeaker(EntityUid uid, EntityUid user, bool quiet = false, RadioSpeakerComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        SetSpeakerEnabled(uid, user, !component.Enabled, quiet, component);
    }

    public void SetSpeakerEnabled(EntityUid uid, EntityUid? user, bool enabled, bool quiet = false, RadioSpeakerComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.Enabled = enabled;
        Dirty(uid, component);

        if (!quiet && user != null)
        {
            var state = GetStatusText(component); // DeltaV - Use method for getting state due to DRY
            var message = Loc.GetString("radio-speaker-component-on-use", ("radioState", state)); // DeltaV - Use locid specific to speaker
            _popup.PopupPredicted(message, user.Value, user.Value); // DeltaV - Make predicted
        }

        _appearance.SetData(uid, RadioDeviceVisuals.Speaker, component.Enabled);
        // BEGIN DeltaV - Extracted to method
        // if (component.Enabled)
        //     EnsureComp<ActiveRadioComponent>(uid).Channels.UnionWith(component.Channels);
        // else
        //     RemCompDeferred<ActiveRadioComponent>(uid);
        UpdateActiveRadioComponent((uid, component));
        //END DeltaV
    }

    /// <summary>
    /// DeltaV - Adds or removes <see cref="ActiveRadioComponent"/> on the entity based on
    /// whether the <see cref="RadioSpeakerComponent"/> is enabled. And copies its channels too.
    /// </summary>
    /// <param name="entity">The entity that owns these components.</param>
    private void UpdateActiveRadioComponent(Entity<RadioSpeakerComponent> entity)
    {
        if (entity.Comp.Enabled)
        {
            var activeRadio = EnsureComp<ActiveRadioComponent>(entity);
            activeRadio.Channels.Clear();
            activeRadio.Channels.UnionWith(entity.Comp.Channels);
        }
        else
            RemCompDeferred<ActiveRadioComponent>(entity);
    }
    #endregion
}

