using Content.Server.Administration.Logs;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.StationEvents.Components;
using Content.Shared._DV.Ghost.Roles;
using Content.Shared.Database;
using Content.Shared.Ghost;
using Content.Shared.Mind.Components;
using Content.Shared.Players;
using Content.Shared.Popups;
using Content.Shared.Station.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._DV.Ghost.Roles;

public sealed class DVSpawnableGhostRoleSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly RoleSystem _role = default!;

    private readonly List<EntityCoordinates> _stationVents = new();
    private readonly List<EntityCoordinates> _allVents = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<DVSpawnableGhostRoleRequestEvent>(OnSpawnRequest);
    }

    private void OnSpawnRequest(DVSpawnableGhostRoleRequestEvent msg, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;
        if (session.AttachedEntity is not { Valid: true } attached
            || !HasComp<GhostComponent>(attached))
        {
            _adminLog.Add(LogType.Action, LogImpact.Medium, $"{session:player} sent {nameof(DVSpawnableGhostRoleRequestEvent)} without being a ghost.");
            return;
        }

        if (!_prototype.TryIndex(msg.Prototype, out var role)
            || !_prototype.TryIndex<EntityPrototype>(role.Entity, out _))
        {
            _adminLog.Add(LogType.Action, LogImpact.Medium, $"{session:player} tried to spawn as invalid {nameof(DVSpawnableGhostRoleRequestEvent)} prototype {msg.Prototype}.");
            return;
        }

        if (!TryPickVent(out var coords))
        {
            _popup.PopupEntity(Loc.GetString("ghost-gui-spawn-vent-critter-no-vents"), attached, attached);
            return;
        }

        var mob = SpawnAtPosition(role.Entity, coords);
        _transform.AttachToGridOrMap(mob);
        EnsureComp<MindContainerComponent>(mob);

        DebugTools.AssertNotNull(session.ContentData());

        if(_mind.TryGetMind(session.UserId, out _, out var mind) && !mind.IsVisitingEntity)
            _mind.WipeMind(session);

        var newMind = _mind.CreateMind(session.UserId, Comp<MetaDataComponent>(mob).EntityName);

        _mind.SetUserId(newMind, session.UserId);
        _mind.TransferTo(newMind, mob);
        _role.MindAddRoles(newMind, role.MindRoles, newMind);

        _adminLog.Add(LogType.Action, LogImpact.Low, $"{session:player} spawned as a vent critter {msg.Prototype}");
    }

    private bool TryPickVent(out EntityCoordinates coords)
    {
        _stationVents.Clear();
        _allVents.Clear();

        var query = EntityQueryEnumerator<VentCritterSpawnLocationComponent, TransformComponent>();
        while (query.MoveNext(out _, out _, out var transform))
        {
            if (!transform.Anchored || !transform.Coordinates.IsValid(EntityManager))
                continue;

            _allVents.Add(transform.Coordinates);

            if (transform.GridUid is { } grid && HasComp<StationMemberComponent>(grid))
                _stationVents.Add(transform.Coordinates);
        }

        if (_stationVents.Count > 0)
        {
            coords = _random.Pick(_stationVents);
            return true;
        }

        if (_allVents.Count > 0)
        {
            coords = _random.Pick(_allVents);
            return true;
        }

        coords = default;
        return false;
    }
}
