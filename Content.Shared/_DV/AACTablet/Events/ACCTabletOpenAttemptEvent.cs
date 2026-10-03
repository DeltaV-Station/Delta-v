namespace Content.Shared._DV.AACTablet.Events;

/// <summary>
/// Cancellable event for attempting to use an AAC tablet, raised on the user.
/// </summary>
/// <param name="Tablet">The tablet which will be opened if successful.</param>
/// <param name="FailReason">The reason why it failed.</param>
/// <param name="Cancelled">Whether the attempt to open it failed.</param>
[ByRefEvent]
public record struct AACTabletOpenAttemptEvent(EntityUid Tablet, LocId? FailReason = null, bool Cancelled = false);
