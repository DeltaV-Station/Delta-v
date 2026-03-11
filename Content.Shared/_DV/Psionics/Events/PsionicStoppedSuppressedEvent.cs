namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// This is raised on an entity that has been psionically suppressed, stopping them from using psionics.
/// </summary>
/// <param name="Victim">The entity that is no longer psionically suppressed.</param>
[ByRefEvent]
public record struct PsionicStoppedSuppressedEvent(EntityUid Victim);
