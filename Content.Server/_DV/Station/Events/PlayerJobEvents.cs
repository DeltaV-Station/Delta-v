using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Station.Events;

/// <summary>
/// Raised on a station when a after a players jobs are removed from the PlayerJobs
/// </summary>
/// <param name="NetUserId">Player whos jobs were removed</param>
/// <param name="PlayerJobs">Entry in PlayerJobs removed a list of JobPrototypes</param>
[ByRefEvent]
public record struct PlayerJobsRemovedEvent(NetUserId NetUserId, List<ProtoId<JobPrototype>> PlayerJobs);

/// <summary>
/// Raised on a staion when a job is added to a player
/// </summary>
/// <param name="NetUserId">Player who recived a job</param>
/// <param name="JobPrototypeId">Id of the jobPrototype added</param>
[ByRefEvent]
public record struct PlayerJobAddedEvent(NetUserId NetUserId, string JobPrototypeId);
