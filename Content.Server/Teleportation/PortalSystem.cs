using Content.Server.Nyanotrasen.Lamiae; //DeltaV
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Ghost;
using Content.Shared.Mind.Components;
using Content.Shared.Nyanotrasen.Lamiae; //DeltaV
using Content.Shared.Teleportation.Systems;
using Robust.Shared.Map;

namespace Content.Server.Teleportation;

public sealed class PortalSystem : SharedPortalSystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly LamiaSystem _lamia = default!; //DeltaV

    // TODO Move to shared
    protected override void LogTeleport(EntityUid portal, EntityUid subject, EntityCoordinates source,
        EntityCoordinates target)
    {
        if (HasComp<MindContainerComponent>(subject) && !HasComp<GhostComponent>(subject))
            _adminLogger.Add(LogType.Teleport, LogImpact.Low, $"{ToPrettyString(subject):player} teleported via {ToPrettyString(portal)} from {source} to {target}");

        //Start DeltaV Code, stops Lamia from crashing because they can't take their tail through a portal
        if (TryComp<LamiaComponent>(subject, out var lamia))
        {
            foreach (var segment in lamia.Segments)
            {
                QueueDel(segment);
            }
            lamia.Segments.Clear();
            _lamia.SpawnSegments(subject, lamia);
        }
        //End DeltaV Code
    }

}
