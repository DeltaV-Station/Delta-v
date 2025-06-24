using Content.Shared.FixedPoint;

namespace Content.Shared._DV.Surgery;

/// <summary>
/// 	Event fired when an object is sterilised for surgery
/// </summary>
[ByRefEvent]
public record struct SurgeryCleanedEvent(FixedPoint2 DirtAmount, int DnaAmount);
