namespace Content.Shared._Shitmed.Body.Events;

/// <summary>
/// Raised on a bodypart or organ before trying to enable it.
/// </summary>
[ByRefEvent]
public record struct MechanismEnableAttemptEvent(EntityUid Body, bool Cancelled = false);

/// <summary>
/// Raised on a bodypart or organ before trying to disable it.
/// Only use this if you know what you are doing.
/// </summary>
[ByRefEvent]
public record struct MechanismDisableAttemptEvent(EntityUid Body, bool Cancelled = false);

/// <summary>
/// Raised on a bodypart or organ after enabling it.
/// </summary>
[ByRefEvent]
public readonly record struct MechanismEnabledEvent(EntityUid Body);

/// <summary>
/// Raised on a bodypart or organ after disabling it.
/// </summary>
[ByRefEvent]
public readonly record struct MechanismDisabledEvent(EntityUid Body);

/// <summary>
/// Raised on a bodypart or organ after installing it in a body.
/// </summary>
[ByRefEvent]
public readonly record struct MechanismAddedEvent(EntityUid Body);

/// <summary>
/// Raised on a bodypart or organ after removing it from a body.
/// </summary>
[ByRefEvent]
public readonly record struct MechanismRemovedEvent(EntityUid Body);
