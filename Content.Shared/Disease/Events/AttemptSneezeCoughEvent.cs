namespace Content.Shared.Disease.Events;

/// <summary>
///     Raised by an entity about to sneeze/cough.
///     Set Cancelled to true on event handling to suppress the sneeze
/// </summary>
[ByRefEvent]
public record struct AttemptSneezeCoughEvent(EntityUid Uid, string? EmoteId, bool Cancelled = false);
