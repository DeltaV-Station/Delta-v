namespace Content.Shared.Nyanotrasen.Lamiae;

public sealed class SegmentSpawnedEvent : EntityEventArgs
{
    public EntityUid Lamia = default!;

    public SegmentSpawnedEvent(EntityUid lamia)
    {
        Lamia = lamia;
    }
}
