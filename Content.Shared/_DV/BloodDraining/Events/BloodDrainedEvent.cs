using Content.Shared.FixedPoint;

namespace Content.Shared._DV.BloodDraining.Events;

/// <summary>
/// Raise when blood has been drained from a victim.
/// Raised against the drainer.
/// </summary>
/// <param name="Victim">Entity which has had their blood drained.</param>
/// <param name="Volume">How much blood was drained.</param>
[ByRefEvent]
public record struct BloodDrainedEvent(
    EntityUid Victim,
    FixedPoint2 Volume
)
{
}
