using Robust.Shared.Network;

namespace Content.Server.DeltaV.Station.Events;

/// <summary>
/// Raised on a staion when a job is added to a player
/// </summary>
/// <param name="netUserId">Player who recived a job</param>
/// <param name="jobPrototypeId">Id of the jobPrototype added</param>
public sealed class PlayerJobAddedEvent(NetUserId netUserId, string jobPrototypeId) : EntityEventArgs
{
    public NetUserId NetUserId = netUserId;
    public string? JobPrototypeId = jobPrototypeId;
}
