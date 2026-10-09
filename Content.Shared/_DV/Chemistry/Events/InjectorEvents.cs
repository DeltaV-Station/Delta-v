namespace Content.Shared._DV.Chemistry.Events;

/// <summary>
/// This event is raised on the injector before the target is injected.
/// </summary>
public sealed class BeforeInjectEvent(EntityUid user, EntityUid target, string? overrideMessage = null) : CancellableEntityEventArgs
{
    public EntityUid EntityUsingInjector = user;
    public EntityUid TargetGettingInjected = target;
    public string? OverrideMessage = overrideMessage;
}
