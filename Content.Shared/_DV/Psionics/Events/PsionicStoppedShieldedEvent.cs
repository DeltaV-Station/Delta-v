namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// This is raised on an entity when they are no longer psionically shielded anymore.
/// </summary>
/// <param name="Shielded">The entity who is no longer psionically shielded.</param>
[ByRefEvent]
public record struct PsionicStoppedShieldedEvent(EntityUid Shielded);
