namespace Content.Shared.Throwing;

/// <summary>
/// Raised directed on the target entity being hit by the thrown entity.
/// </summary>
[ByRefEvent]
public sealed class ThrowHitByEvent(EntityUid thrown, EntityUid target, ThrownItemComponent component) : HandledEntityEventArgs
{
    public EntityUid Thrown = thrown;

    public EntityUid Target = target;

    public ThrownItemComponent Component = component;
}

/// <summary>
/// Raised directed on the thrown entity that hits another.
/// </summary>
[ByRefEvent]
public sealed class ThrowDoHitEvent(EntityUid thrown, EntityUid target, ThrownItemComponent component) : HandledEntityEventArgs
{
    public EntityUid Thrown = thrown;

    public EntityUid Target = target;

    public ThrownItemComponent Component = component;
}
