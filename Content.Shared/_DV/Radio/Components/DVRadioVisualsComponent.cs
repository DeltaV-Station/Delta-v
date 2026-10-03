using Content.Shared.Hands.Components;

namespace Content.Shared._DV.Radio.Components;

/// <summary>
/// Component visualizing
/// </summary>
[RegisterComponent]
public sealed partial class DVRadioVisualsComponent : Component
{
    /// <summary>
    /// Sprite layer that will have its visibility toggled based on <see cref="RadioMicrophoneComponent"/>.
    /// </summary>
    [DataField]
    public string? MicrophoneSpriteLayer;

    /// <summary>
    /// Sprite layer that will have its visibility toggled based on <see cref="RadioSpeakerComponent"/>.
    /// </summary>
    [DataField]
    public string? SpeakerSpriteLayer;

    /// <summary>
    /// Layers to add to the sprite of the player that is holding this entity when its microphone is on.
    /// </summary>
    [DataField]
    public Dictionary<HandLocation, List<PrototypeLayerData>> MicrophoneInhandVisuals = new();

    /// <summary>
    /// Layers to add to the sprite of the player that is holding this entity when its speaker is on.
    /// </summary>
    [DataField]
    public Dictionary<HandLocation, List<PrototypeLayerData>> SpeakerInhandVisuals = new();
}
