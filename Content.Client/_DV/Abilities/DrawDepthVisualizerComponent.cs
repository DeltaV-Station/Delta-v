using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client._DV.Abilities;

/// <summary>
/// Changes the sprite's draw depth when some appearance data becomes true, reverting it when false.
/// </summary>
[RegisterComponent]
public sealed partial class DrawDepthVisualizerComponent : Component
{
    /// <summary>
    /// Appearance key to check.
    /// </summary>
    [DataField(required: true)]
    public Enum Key;

    /// <summary>
    /// The draw depth to set the sprite to when the appearance data is true.
    /// </summary>
    [DataField(required: true)]
    public DrawDepth Depth;

    [DataField]
    public int? OriginalDrawDepth;
}
