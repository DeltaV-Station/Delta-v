using Content.Server._DV.StationEvents.Events;

namespace Content.Server._DV.StationEvents.Components;

/// <summary>
/// Attempts to change the hair and facial hair markings, plus their color
/// of a small amount of people.
/// </summary>
[RegisterComponent, Access(typeof(GlimmerRestyleRule))]
public sealed partial class GlimmerRestyleRuleComponent : Component
{
    /// <summary>
    /// Minimum number of valid targets that will get restyled.
    /// </summary>
    [DataField]
    public int MinimumTargets = 1;

    /// <summary>
    /// Maximum number of valid targets that will get restyled.
    /// </summary>
    [DataField]
    public int MaximumTargets = 5;

    /// <summary>
    /// Chance of completely removing all hair markings instead of selecting a random one.
    /// </summary>
    [DataField]
    public float BaldChance = 0.2f;

    /// <summary>
    /// Chance of completely removing all facial hair markings instead of selecting a random one.
    /// </summary>
    [DataField]
    public float CleanShavenChance = 0.5f;
}
