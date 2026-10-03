namespace Content.Shared._DV.Throwing;

/// <summary>
/// Enables something to catch items with their hands if possible.
/// </summary>
[ByRefEvent]
public sealed class ThrowDamageAttemptedEvent : CancellableEntityEventArgs
{
    public EntityUid? Thrown;
}
