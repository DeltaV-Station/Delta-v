namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// This is used for things that do something special when they are hit by telekinesis, like simple buttons and switches.
/// </summary>
[ByRefEvent]
public sealed class TelekinesisUsedEvent(EntityUid target, EntityUid performer) : CancellableEntityEventArgs
{
    public EntityUid Target { get; } = target;

    public EntityUid User { get; } = performer;
};
