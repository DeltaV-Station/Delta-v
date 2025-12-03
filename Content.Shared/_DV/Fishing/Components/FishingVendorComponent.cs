using Robust.Shared.GameStates;

namespace Content.Shared._DV.Fishing.Components;

/// <summary>
/// Vendor that exchanges fish for fishing points.
/// Awards different points based on fish rarity (difficulty threshold).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FishingVendorComponent : Component
{
    /// <summary>
    /// Points awarded for base (common) fish.
    /// </summary>
    [DataField]
    public uint BaseFishPoints = 10;

    /// <summary>
    /// Points awarded for rare fish.
    /// </summary>
    [DataField]
    public uint RareFishPoints = 25;

    /// <summary>
    /// Difficulty threshold to distinguish rare fish from base fish.
    /// Fish with difficulty >= this value are considered rare.
    /// </summary>
    [DataField]
    public float RareFishThreshold = 0.035f;
}
