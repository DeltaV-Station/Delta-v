namespace Content.Shared._DV.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class AmmoDelayComponent : Component
{
    /// <summary>
    /// Flat delay to apply
    /// </summary>
    [DataField]
    public float? Delay;

    /// <summary>
    /// Minimum random delay, this doesn't matter if delay isn't null
    /// </summary>
    [DataField]
    public float MinimumDelay = 1.0f;

    /// <summary>
    /// Maximum random delay, this doesn't matter if delay isn't null
    /// </summary>
    [DataField]
    public float MaximumDelay = 1.0f;
}
