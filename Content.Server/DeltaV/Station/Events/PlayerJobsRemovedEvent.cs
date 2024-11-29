using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Server.DeltaV.Station.Events;

/// <summary>
/// Raised on a station when a players jobs are removed
/// </summary>
/// <param name="netUserId">Player whos jobs were removed</param>
/// <param name="playerJobs">Entry in PlayerJobs removed a list of JobPrototypes</param>
public sealed class PlayerJobsRemovedEvent(NetUserId netUserId, List<ProtoId<JobPrototype>> playerJobs) : EntityEventArgs
{
    public NetUserId NetUserId = netUserId;
    public List<ProtoId<JobPrototype>>? PlayerJobs = playerJobs;
}
