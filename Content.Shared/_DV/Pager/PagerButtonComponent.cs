using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Pager;

/// <summary>
///     Sends out a page when pressed.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedPagerButtonSystem))]
public sealed partial class PagerButtonComponent : Component
{
    /// <summary>
    /// The sound to play when pressed.
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier PressSound;

    /// <summary>
    /// The state to use for the press animation.
    /// </summary>
    [DataField(required: true)]
    public string PressState;

    /// <summary>
    /// The state to use after the press animation.
    /// </summary>
    [DataField(required: true)]
    public string UnpressedState;

    /// <summary>
    /// The message to send via the pager system. Has "location" passed in as an argument.
    /// </summary>
    [DataField(required: true)]
    public LocId PageMessage;
}

public enum PagerButtonVisualLayers : byte
{
    Base,
}
