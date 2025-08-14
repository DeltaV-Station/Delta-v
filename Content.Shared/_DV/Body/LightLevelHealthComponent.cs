using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Shared._DV.Body;

/// <summary>
/// Component that allows a body to have health that is affected by light levels.
/// Either damaged or healed by certain light levels.
/// This is used for the Skia, which is a creature that is harmed by light.
/// </summary>
[RegisterComponent]
public sealed partial class LightLevelHealthComponent : Component
{
    /// <summary>
    /// Level of light that, when below, we are considered in darkness.
    /// </summary>
    [DataField]
    public float DarkThreshold = 0.2f;
    /// <summary>
    /// Level of light that, when above, we are considered in light.
    /// </summary>
    [DataField]
    public float LightThreshold = 0.8f;
    /// <summary>
    /// Amount of health or damage per second when in darkness. Positive values harm, negative values heal.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier DarkDamage = default!;
    /// <summary>
    /// Amount of health or damage per second when in light. Positive values harm, negative values heal.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier LightDamage = default!;
    /// <summary>
    /// Movement speed multiplier when in darkness.
    /// </summary>
    [DataField]
    public float DarkMovementSpeedMultiplier = 1.0f;
    /// <summary>
    /// Movement speed multiplier when in light.
    /// </summary>
    [DataField]
    public float LightMovementSpeedMultiplier = 1.0f;
    /// <summary>
    /// Sound to play when the entity is damaged by light or darkness.
    /// </summary>
    [DataField]
    public SoundSpecifier SizzleSoundPath = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    /// <summary>
    /// The current light threshhold for this component.
    /// -1 for darkness, 1 for light.
    /// 0 for neither.
    /// </summary>
    [DataField]
    public int CurrentThreshold = 0;
}
