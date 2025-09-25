using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Speech.Prototypes;

[Prototype]
public sealed partial class SyllableObfuscationAccentPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public int MinSyllables = 1;

    [DataField]
    public int MaxSyllables = 4;

    [DataField(required: true)]
    public List<string> Replacement = [];
}
