namespace Content.Shared._DV.Polymorph;

/// <summary>
/// Raised directed on an entity before polymorphing it.
/// Cancel to stop the entity from being polymorphed.
/// </summary>
/// <param name="Target">The entity that is being polymorphed into.</param>
[ByRefEvent]
public record struct BeforePolymorphedEvent(EntityUid Target, bool Cancelled = false);
