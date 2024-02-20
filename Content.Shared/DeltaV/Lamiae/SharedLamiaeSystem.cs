namespace Content.Shared.DeltaV.Lamiae;

public sealed class SegmentSpawnedEvent : EntityEventArgs
{
    public EntityUid Lamia = default!;

    public SegmentSpawnedEvent(EntityUid lamia)
    {
        Lamia = lamia;
    }
}
