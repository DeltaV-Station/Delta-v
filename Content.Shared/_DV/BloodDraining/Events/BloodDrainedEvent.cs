using Content.Shared.FixedPoint;

namespace Content.Shared._DV.BloodDraining.Events;

[ByRefEvent]
public record struct BloodDrainedEvent(
    EntityUid Victim,
    FixedPoint2 Volume
)
{
}
