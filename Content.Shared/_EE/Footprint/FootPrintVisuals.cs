using Robust.Shared.Serialization;

namespace Content.Shared._EE.FootPrint;

/// <summary>
/// Visual states for different types of footprints.
/// </summary>
[Serializable, NetSerializable]
public enum FootPrintVisuals : byte
{
    /// <summary>
    /// Bare foot footprints (no shoes).
    /// </summary>
    BareFootPrint,

    /// <summary>
    /// Regular shoe footprints.
    /// </summary>
    ShoesPrint,

    /// <summary>
    /// Hardsuit footprints.
    /// </summary>
    SuitPrint,

    /// <summary>
    /// Drag marks from being pulled/dragged.
    /// </summary>
    Dragging
}

/// <summary>
/// Appearance data keys for footprint visuals.
/// </summary>
[Serializable, NetSerializable]
public enum FootPrintVisualState : byte
{
    /// <summary>
    /// The current visual state (type of print).
    /// </summary>
    State,

    /// <summary>
    /// The color of the footprint.
    /// </summary>
    Color
}

/// <summary>
/// Sprite layers for footprint rendering.
/// </summary>
[Serializable, NetSerializable]
public enum FootPrintVisualLayers : byte
{
    /// <summary>
    /// The footprint sprite layer.
    /// </summary>
    Print
}
