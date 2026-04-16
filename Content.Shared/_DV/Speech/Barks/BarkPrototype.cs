using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Speech.Barks;

/// <summary>
/// Defining speech bark voices, and which sounds to play based on character speech.
/// </summary>
[Prototype]
public sealed partial class BarkPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Sound collection to pull the blips from
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier Sounds = default!;

    /// <summary>
    /// Pitch multiplier, minimum
    /// </summary>
    [DataField]
    public float MinPitch = 0.9f;

    /// <summary>
    /// Pitch multiplier, maximum
    /// </summary>
    [DataField]
    public float MaxPitch = 1.1f;

    /// <summary>
    /// Base volume. Negative = quiet. Positive = loud
    /// </summary>
    [DataField]
    public float Volume = 0f;
    
    /// <summary>
    /// Blips per letter.
    /// </summary>
    [DataField]
    public float Frequency = 1f;

    /// <summary>
    /// Determine if pitch and sound is the same relative to a character.
    /// </summary>
    [DataField]
    public bool Predictable = true;

    /// <summary>
    /// Species to use Barks. Null makes it available to all.
    /// </summary>
    [DataField]
    public List<string>? SpeciesWhitelist;
}
