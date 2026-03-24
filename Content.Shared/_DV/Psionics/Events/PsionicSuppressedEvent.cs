namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// Raised on an entity when they are no longer psionically suppressed from using psionic abilities.
/// </summary>
/// <param name="Victim">The entity who is psionically suppressed</param>
[ByRefEvent]
public record struct PsionicSuppressedEvent(EntityUid Victim);
