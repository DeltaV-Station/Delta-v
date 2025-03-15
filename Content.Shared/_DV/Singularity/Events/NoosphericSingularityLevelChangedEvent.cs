using Content.Shared._DV.Singularity.Components;

namespace Content.Shared._DV.Singularity.Events;

/// <summary>
/// An event raised whenever a singularity changes its level.
/// </summary>
public sealed class NoosphericSingularityLevelChangedEvent(
    byte newValue,
    byte oldValue,
    NoosphericSingularityComponent singularity) : EntityEventArgs
{
    /// <summary>
    /// The new level of the singularity.
    /// </summary>
    public readonly byte NewValue = newValue;

    /// <summary>
    /// The previous level of the singularity.
    /// </summary>
    public readonly byte OldValue = oldValue;

    /// <summary>
    /// The singularity that just changed level.
    /// </summary>
    public readonly NoosphericSingularityComponent Singularity = singularity;
}
