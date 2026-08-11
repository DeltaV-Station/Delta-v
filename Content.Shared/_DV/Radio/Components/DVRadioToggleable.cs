namespace Content.Shared._DV.Radio.Components;

/// <summary>
/// Parent class for RadioMicrophoneComponent and RadioSpeakerComponent,
/// because they're basically have all the same fields...
/// </summary>
public abstract partial class DVRadioToggleable : Component
{
    /// <summary>
    /// Whether it's on or off :)
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = false;

    /// <summary>
    /// Whether clicking the entity should toggle the Enable field.
    /// </summary>
    [DataField]
    public bool ToggleOnInteract = false;

    /// <summary>
    /// Whether or not this entity has a verb
    /// for toggling this component on or off.
    /// </summary>
    [DataField]
    public bool Toggleable = true;

    /// <summary>
    /// Text to show in the verb menu for the "Enable" action.
    /// </summary>
    [DataField]
    public abstract LocId EnableVerbText { get; set; }

    /// <summary>
    /// Text to show in the verb menu for the "Disable" action.
    /// </summary>
    [DataField]
    public abstract LocId DisableVerbText { get; set; }
}

