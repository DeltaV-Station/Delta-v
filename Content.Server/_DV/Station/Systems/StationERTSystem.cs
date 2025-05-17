using Content.Server._DV.Administration;
using Content.Server._DV.Station.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Verbs;
using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Content.Shared._DV.ERT;
using Content.Shared.Administration;
using Content.Shared.Database;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._DV.Station.Systems;

public sealed class StationERTSystem : EntitySystem
{
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly IAdminManager _admin = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationERTComponent, GetVerbsEvent<Verb>>(OnGetERTVerbs);
    }

    private void OnGetERTVerbs(Entity<StationERTComponent> entity, ref GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        var session = actor.PlayerSession;
        if (!_admin.HasAdminFlag(session, AdminFlags.Spawn))
            return;

        args.Verbs.Add(new Verb()
        {
            Text = Loc.GetString("admin-verb-text-send-ert"),
            Category = VerbCategory.Admin,
            Act = () =>
            {
                _eui.OpenEui(new SendERTEui(GetNetEntity(entity)), session);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-description-send-ert"),
        });
    }

    public void SendERT(EntityUid station,
        ProtoId<ERTTeamPrototype> team,
        Dictionary<ProtoId<ERTRolePrototype>, int> composition)
    {
        if (!TryComp<StationERTComponent>(station, out var stationErt))
            return;

        if (!_prototype.TryIndex(team, out var ertTeam))
            return;

        foreach (var role in composition.Keys)
        {
            if (!ertTeam.Roles.ContainsKey(role))
                composition.Remove(role);
        }

        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePoints = new List<EntityCoordinates>();
        while (points.MoveNext(out var uid, out var spawnPoint, out var xform))
        {
            if (_station.GetOwningStation(uid, xform) != station)
                continue;

            if (spawnPoint.SpawnType != SpawnPointType.LateJoin)
                continue;

            possiblePoints.Add(xform.Coordinates);
        }

        if (possiblePoints.Count == 0)
            return;

        foreach (var (role, count) in composition)
        {
            for (var i = 0; i < count; i++)
            {
                var spawnLoc = _random.Pick(possiblePoints);

                Spawn(ertTeam.Roles[role], spawnLoc);
            }
        }
    }
}
