using Content.Shared._DV.Speech.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._DV.Speech.Components;

[RegisterComponent]
public sealed partial class SyllableObfuscationAccentComponent : Component
{
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<SyllableObfuscationAccentPrototype>), required: true)]
    public string Accent = default!;
}
