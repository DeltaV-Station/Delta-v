using System.Diagnostics.CodeAnalysis;
using Content.Server.Administration.Logs;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.StationEvents.Components;
using Content.Shared._DV.CCVars;
using Content.Shared._DV.Ghost.Roles;
using Content.Shared.Database;
using Content.Shared.Ghost;
using Content.Shared.Mind.Components;
using Content.Shared.Players;
using Content.Shared.Popups;
using Content.Shared.Station.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
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
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    private readonly List<EntityCoordinates> _stationVents = new();
    private readonly List<EntityCoordinates> _allVents = new();
    private readonly Dictionary<NetUserId, TimeSpan> _cooldowns = new();
    private TimeSpan _spawnCooldown;

    public override void Initialize()
    {
        base.Initialize();

        _player.PlayerStatusChanged += OnPlayerStatusChanged;

        Subs.CVar(_cfg, DCCVars.VentCritterGhostRoleSpawnCooldown, value => _spawnCooldown = value < TimeSpan.Zero ? TimeSpan.Zero : value, true);

        SubscribeNetworkEvent<DVSpawnableGhostRoleRequestEvent>(OnSpawnRequest);
        SubscribeNetworkEvent<DVSpawnableGhostRoleCooldownRequestEvent>(OnCooldownRequest);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _player.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs evt)
    {
        if (evt.NewStatus == SessionStatus.Connected)
        {
            SendCooldownUpdate(evt.Session);
        }
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

        if (TestCooldown(session.UserId, out var cooldownEnd))
        {
            SendCooldownUpdate(session);
            var remaining = Math.Ceiling((cooldownEnd.Value - _timing.CurTime).TotalSeconds);
            _popup.PopupEntity(Loc.GetString("ghost-gui-spawn-vent-critter-cooldown-popup", ("time", remaining)), attached, attached);
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

        _cooldowns[session.UserId] = _timing.CurTime + _spawnCooldown;
        SendCooldownUpdate(session);

        _adminLog.Add(LogType.Action, LogImpact.Low, $"{session:player} spawned as a vent critter {msg.Prototype}");
    }

    private void OnCooldownRequest(DVSpawnableGhostRoleCooldownRequestEvent msg, EntitySessionEventArgs args)
    {
        SendCooldownUpdate(args.SenderSession);
    }

    /// <param name="userId">The user to test for</param>
    /// <param name="cooldownEndsAt">When the cooldown will end</param>
    /// <returns>If the user is on cooldown</returns>
    private bool TestCooldown(NetUserId userId, [NotNullWhen(true)] out TimeSpan? cooldownEndsAt)
    {
        if (_cooldowns.TryGetValue(userId, out var cooldownEnd))
        {
            cooldownEndsAt = cooldownEnd;
            if (_timing.CurTime < cooldownEnd)
                return true;

            _cooldowns.Remove(userId);
        }

        cooldownEndsAt = null;
        return false;
    }

    private void SendCooldownUpdate(ICommonSession session)
    {
        TestCooldown(session.UserId, out var cooldownEnd);
        RaiseNetworkEvent(new DVSpawnableGhostRoleCooldownUpdateEvent(cooldownEnd), session.Channel);
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
