using Robust.Shared.GameStates;

namespace Content.Shared._DV.Overlays.Components;

/// <summary>
/// Gives the owner darkvision: lighting still renders, but total darkness is raised to
/// <see cref="LightFloor"/> brightness instead of pitch black. Unlike night vision this keeps
/// the whole lighting gradient visible, so creatures like the Skia can judge what is and isn't dark enough for them.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DarkVisionComponent : Component
{
    /// <summary>
    /// Brightness that full darkness renders at, 0-1. Rendered light is clamped to a minimum of this value.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float LightFloor = 0.2f;

    /// <summary>
    /// Multiplier applied to actual light on top of the floor. Values above 1 overbrighten lit areas so they are unmistakable next to the grey darkness floor.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float LightGain = 8f;

    /// <summary>
    /// Exponent applied to lights, to make brighter areas look notably brighter
    /// </summary>
    [DataField, AutoNetworkedField]
    public float LightExp = 2f;
}
