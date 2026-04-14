using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Speech.Barks;

// Defining speech bark voices
[Prototype]
public sealed partial class BarkPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    // Sound collection to pull the blips from
    [DataField(required: true)]
    public SoundSpecifier Sounds = default!;

    // Pitch multiplier, minimum
    [DataField]
    public float MinPitch = 0.9F;

    // Pitch multiplier, maximum
    [DataField]
    public float MaxPitch = 1.1F;

    // Base volume. Negative = quiet. Positive = loud
    [DataField]
    public float Volume = 0F;

    // Blips per letter.
    [DataField]
    public float Frequency = 1F;

    // Determine if pitch and sound is the same relative to a character.
    [DataField]
    public bool Predictable = true;

    // Species to use Barks. Null makes it available to all.
    [DataField]
    public List<string>? SpeciesWhitelist;
}
