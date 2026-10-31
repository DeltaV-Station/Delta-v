namespace Content.Shared.Throwing;

/// <summary>
/// Raised on an entity after it has thrown something.
/// </summary>
[ByRefEvent]
public readonly record struct ThrowEvent(EntityUid? User, EntityUid Thrown);

/// <summary>
/// Raised on an entity after it has been thrown.
/// </summary>
[ByRefEvent]
public readonly record struct ThrownEvent(EntityUid? User, EntityUid Thrown);

/// <summary>
/// Raised directed on the target entity being hit by the thrown entity.
/// </summary>
[ByRefEvent]
public sealed class ThrowHitByEvent(EntityUid thrown, EntityUid target, ThrownItemComponent component) : HandledEntityEventArgs // Delta-V
{
    public EntityUid Thrown = thrown;

    public EntityUid Target = target;

    public ThrownItemComponent Comp = component;
}

/// <summary>
/// Raised directed on the thrown entity that hits another.
/// </summary>
[ByRefEvent]
public sealed class ThrowDoHitEvent(EntityUid thrown, EntityUid target, ThrownItemComponent component) : HandledEntityEventArgs // Delta-V
{
    public EntityUid Thrown = thrown;

    public EntityUid Target = target;

    public ThrownItemComponent Comp = component;
}
