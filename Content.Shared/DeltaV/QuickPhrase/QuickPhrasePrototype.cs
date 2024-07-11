using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Server.DeltaV.QuickPhrase;

[Prototype("quickPhrase")]
public sealed partial class QuickPhrasePrototype : IPrototype, IInheritingPrototype
{
    /// <summary>
    /// The "in code name" of the object. Must be unique.
    /// </summary>
    [ViewVariables]
    [IdDataFieldAttribute]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The prototype we inherit from.
    /// </summary>
    [ViewVariables]
    [ParentDataFieldAttribute(typeof(AbstractPrototypeIdArraySerializer<QuickPhrasePrototype>))]
    public string[]? Parents { get; }

    [ViewVariables]
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; }

    /// <summary>
    /// The phrase that this prototype represents.
    /// </summary>
    [DataField("text")]
    public string Text = string.Empty;

    /// <summary>
    /// Determines how the phrase is sorted in the UI.
    /// </summary>
    [DataField("group")]
    public string Group = string.Empty;

    /// <summary>
    /// The tab in the UI that this phrase falls under.
    /// </summary>
    [DataField("tab")]
    public string Tab = string.Empty;

    /// <summary>
    /// Color of button in UI.
    /// </summary>
    [DataField("color")]
    public Color Color { get; private set; } = Color.White;
}