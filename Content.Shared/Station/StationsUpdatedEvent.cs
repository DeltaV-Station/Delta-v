using System.ComponentModel.DataAnnotations;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Serialization;

namespace Content.Shared.Station;

// delta-v
[NetSerializable, Serializable]
public record struct StationRecord(string StationName, NetEntity StationEntity, List<NetEntity> StationGrids)
{
    public static implicit operator ValueTuple<string, NetEntity>(StationRecord record)
    {
        return (record.StationName, record.StationEntity);
    }
};

// end delta-v
[NetSerializable, Serializable]
public sealed class StationsUpdatedEvent : EntityEventArgs
{
    public readonly List<StationRecord> Stations;

    public StationsUpdatedEvent(List<StationRecord> stations)
    {
        Stations = stations;
    }
}
