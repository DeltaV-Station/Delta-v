using System.Runtime.InteropServices;
using Content.Server.Ghost.Roles.Components;
using Content.Shared._Corvax.Respawn;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Content.Shared._NF.CCVar; // Frontier
using Robust.Shared.Configuration; // Frontier
using Robust.Shared.Player; // Frontier
using Content.Shared.Ghost; // Frontier
using Content.Server.Administration.Managers; // Frontier
using Content.Server.Administration; // Frontier
using Content.Shared.GameTicking; // Frontier

namespace Content.Server._Corvax.Respawn;

public sealed class RespawnSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IAdminManager _admin = default!;

    private float _respawnTime = 0f;

    // Frontier: struct for respawn lookup
    private sealed class RespawnData
    {
        public TimeSpan RespawnTime; // The next time the user can respawn.
    }
    // End Frontier

    [ViewVariables]
    private Dictionary<NetUserId, RespawnData> _respawnInfo = new(); // Frontier: struct for complete respawn info
    public override void Initialize()
    {
        SubscribeLocalEvent<MindContainerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MindContainerComponent, MindRemovedMessage>(OnMindRemoved);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart); // Frontier

        _admin.OnPermsChanged += OnAdminPermsChanged; // Frontier
        _player.PlayerStatusChanged += PlayerStatusChanged; // Frontier

        Subs.CVar(_cfg, NFCCVars.RespawnTime, OnRespawnCryoTimeChanged, true); // Frontier
    }

    // Frontier: CVar setters
    private void OnRespawnCryoTimeChanged(float value)
    {
        _respawnTime = value;
    }
    // End Frontier

    private void OnMobStateChanged(EntityUid entity, MindContainerComponent component, MobStateChangedEvent e)
    {
        if (e.NewMobState != MobState.Dead)
            return;

        if (!_player.TryGetSessionByEntity(entity, out var session))
            return;

        var respawnData = GetRespawnData(session.UserId);
        SetRespawnTime(session.UserId, ref respawnData, _timing.CurTime + TimeSpan.FromSeconds(_respawnTime));
    }

    private void OnMindRemoved(EntityUid entity, MindContainerComponent _, MindRemovedMessage e)
    {
        if (e.Mind.Comp.UserId is null)
            return;

        // Mob is dead, don't reset spawn timer twice
        if (TryComp<MobStateComponent>(entity, out var state) && state.CurrentState == MobState.Dead)
            return;

        // Frontier: extra conditions for respawn lenience
        if (HasComp<GhostRoleComponent>(entity)) // Don't penalize user for exiting ghost roles
            return; // Frontier: don't penalize user for exiting ghost roles

        if (HasComp<GhostComponent>(entity)) // Don't penalize user for reobserving
            return;

        if (e.Mind.Comp.Session != null && _admin.IsAdmin(e.Mind.Comp.Session)) // Admins get free respawns
            return;

        // Get respawn info
        var userId = e.Mind.Comp.UserId.Value;
        var respawnInfo = GetRespawnData(userId);
        // End Frontier

        SetRespawnTime(userId, ref respawnInfo, _timing.CurTime + TimeSpan.FromSeconds(_respawnTime));
    }

    // Frontier: admin permissions handler: clear respawn data for admins
    private void OnAdminPermsChanged(AdminPermsChangedEventArgs args)
    {
        if (args.IsAdmin)
        {
            var respawnData = GetRespawnData(args.Player.UserId);
            SetRespawnTime(args.Player.UserId, ref respawnData, TimeSpan.Zero);
        }
    }

    // Frontier: respawn handler: adjusts respawn and cryo timers.
    public void Respawn(ICommonSession session)
    {
        var respawnData = GetRespawnData(session.UserId);
    }

    private void SetRespawnTime(NetUserId user, ref RespawnData data, TimeSpan nextSpawn, TimeSpan? cryoTime = null) // Frontier: Reset<Set, added cryoTime, time changed to be time of next respawn, not time of death
    {
        data.RespawnTime = nextSpawn;

        if (_player.TryGetSessionById(user, out var session)) // Frontier: try first, if no valid session, nothing to do.
            RaiseNetworkEvent(new RespawnResetEvent(nextSpawn), session);
    }

    public TimeSpan? GetRespawnTime(NetUserId user) // Frontier: GetRespawnResetTime<GetRespawnTime
    {
        return _respawnInfo.TryGetValue(user, out var data) ? data.RespawnTime : null;
    }

    // Frontier: return a writable reference
    private ref RespawnData GetRespawnData(NetUserId player)
    {
        if (!_respawnInfo.ContainsKey(player))
            _respawnInfo[player] = new RespawnData();
        return ref CollectionsMarshal.GetValueRefOrNullRef(_respawnInfo, player);
    }

    // Frontier: send ghost timer on player connection
    private void PlayerStatusChanged(object? _, SessionStatusEventArgs args)
    {
        var session = args.Session;

        if (args.NewStatus == Robust.Shared.Enums.SessionStatus.InGame &&
            _respawnInfo.ContainsKey(session.UserId))
        {
            RaiseNetworkEvent(new RespawnResetEvent(_respawnInfo[session.UserId].RespawnTime), session);
        }
    }

    // Frontier: reset game state, we have a new round.
    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _respawnInfo.Clear();
    }
    // End Frontier
}
