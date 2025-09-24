using Robust.Shared.Prototypes;

namespace Content.Server._DV.Speech.Prototypes;

[Prototype("syllableObfuscationAccent")]
public sealed partial class SyllableObfuscationAccentPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public int MinSyllables = 1;

    [DataField]
    public int MaxSyllables = 4;

    [DataField]
    public List<String> Replacement = new() { "<?>" };
}
