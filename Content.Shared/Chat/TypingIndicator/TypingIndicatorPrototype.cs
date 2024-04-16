using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Chat.TypingIndicator;

/// <summary>
///     Prototype to store chat typing indicator visuals.
/// </summary>
[Prototype("typingIndicator")]
public sealed partial class TypingIndicatorPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("spritePath")]
    public ResPath SpritePath = new("/Textures/Effects/speech.rsi");

    [DataField("typingState", required: true)]
    public string TypingState = default!;

    [DataField("offset")]
    public Vector2 Offset = new(0, 0);

    [DataField("shader")]
    public string Shader = "shaded";

    /// <summary>
    /// Delta-V: Sprite path for synth variant of talk sprite.
    /// </summary>
    [DataField]
    public ResPath SynthSpritePath = new("/Textures/DeltaV/Effects/speech_synth.rsi");

    /// <summary>
    /// Delta-V: Whether there is a synth variant for this talk sprite.
    /// </summary>
    [DataField]
    public bool HasSynthVariant;
}
