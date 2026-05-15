using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Damage;

/// <summary>
/// A constant value to reference in <see cref="DVDamageValueDictionarySerializer" />
/// </summary>
[Prototype("dvDamageConstant", 10)]
public sealed partial class DVDamageConstantPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public float Value;
}
