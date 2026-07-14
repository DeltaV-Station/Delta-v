namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// This is raised on an entity when they are psionically shielded.
/// </summary>
/// <param name="Shielded">The entity that is psionically shielded.</param>
[ByRefEvent]
public record struct PsionicShieldedEvent(EntityUid Shielded);
