/*
* Delta-V - This file is licensed under AGPLv3
* Copyright (c) 2024 Delta-V Contributors
* See AGPLv3.txt for details.
*/

namespace Content.Shared.DeltaV.Lamiae;

public sealed class SegmentSpawnedEvent : EntityEventArgs
{
    public EntityUid Lamia = default!;

    public SegmentSpawnedEvent(EntityUid lamia)
    {
        Lamia = lamia;
    }
}
