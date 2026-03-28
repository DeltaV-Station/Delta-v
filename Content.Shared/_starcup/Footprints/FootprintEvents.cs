using Robust.Shared.Serialization;

namespace Content.Shared._starcup.Footprints;

/// <summary>
/// Raised when a cleaning action happens on a tile with footprints
/// </summary>
public readonly struct FootprintCleanEvent;

/// <summary>
/// Raised on an entity when new footprints have been added to the footprint
/// </summary>
[Serializable, NetSerializable]
public sealed class FootprintChangedEvent(NetEntity entity) : EntityEventArgs
{
    public NetEntity Entity = entity;
}
