using Content.Server.Database;
using Content.Server.Mind;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Roles.Jobs;
using Content.Server.Tips;
using Content.Shared._DV.Tips;
using Content.Shared._DV.Tips.Conditions;
using Content.Shared.GameTicking;
using Content.Shared.Roles.Components;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._DV.Tips;

/// <summary>
/// Server-side system that handles showing tips to players after they spawn alongside validation.
/// Not to be confused with <see cref="TipsSystem"/>.
/// </summary>
public sealed class TipSystem : SharedTipSystem
{
    [Dependency] private readonly IComponentFactory _component = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly JobSystem _job = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playtime = default!;

    /// <summary>
    /// Tracks scheduled tips for each player session.
    /// </summary>
    private readonly Dictionary<ICommonSession, List<ScheduledTip>> _scheduledTips = new();

    /// <summary>
    /// Cache of seen tips per player to avoid repeated DB queries.
    /// </summary>
    private readonly Dictionary<NetUserId, HashSet<string>> _seenTipsCache = new();

    private readonly List<ICommonSession> _toRemove = new();

    private sealed class ScheduledTip
    {
        public ProtoId<TipPrototype> TipId;
        public TimeSpan ShowTime;
        public EntityUid PlayerEntity;
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeNetworkEvent<TipDismissedEvent>(OnTipDismissed);
        SubscribeNetworkEvent<ResetAllSeenTipsRequest>(OnResetAllSeenTipsRequest);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _timing.CurTime;
        _toRemove.Clear();

        foreach (var (session, tips) in _scheduledTips)
        {
            if (session.Status != SessionStatus.InGame)
            {
                _toRemove.Add(session);
                continue;
            }

            for (var i = tips.Count - 1; i >= 0; i--)
            {
                var scheduled = tips[i];

                if (currentTime < scheduled.ShowTime)
                    continue;

                // Re-check conditions at show time in case state changed
                if (Prototype.TryIndex(scheduled.TipId, out var tipProto) &&
                    CheckConditions(scheduled.PlayerEntity, session, tipProto))
                {
                    ShowTip(session, tipProto);
                }

                tips.RemoveAt(i);
            }

            if (tips.Count == 0)
                _toRemove.Add(session);
        }

        foreach (var session in _toRemove)
        {
            _scheduledTips.Remove(session);
        }
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        if (ev.Player is not { } session)
            return;

        ScheduleTipsForPlayer(session, ev.Mob);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _scheduledTips.Clear();
        _seenTipsCache.Clear();
    }

    private void OnTipDismissed(TipDismissedEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession is not { } session)
            return;

        // Only persist if the player checked "Don't show again"
        if (!ev.DontShowAgain)
            return;

        MarkTipSeen(session.UserId, ev.TipId);
    }

    private void OnResetAllSeenTipsRequest(ResetAllSeenTipsRequest ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession is not { } session)
            return;

        ResetAllSeenTips(session.UserId);
        Log.Info($"Player {session.Name} reset all seen tips.");
    }

    /// <summary>
    /// Marks a tip as seen for a player, updating both cache and database.
    /// </summary>
    public async void MarkTipSeen(NetUserId player, ProtoId<TipPrototype> tipId)
    {
        // Update cache immediately
        if (!_seenTipsCache.TryGetValue(player, out var seenTips))
        {
            seenTips = new HashSet<string>();
            _seenTipsCache[player] = seenTips;
        }

        seenTips.Add(tipId.Id);

        // Persist to database
        await _db.MarkTipSeen(player, tipId);
    }

    /// <summary>
    /// Resets a seen tip for a player, allowing it to be shown again.
    /// </summary>
    public async void ResetSeenTip(NetUserId player, ProtoId<TipPrototype> tipId)
    {
        // Update cache
        if (_seenTipsCache.TryGetValue(player, out var seenTips))
        {
            seenTips.Remove(tipId.Id);
        }

        // Update database
        await _db.ResetSeenTip(player, tipId);
    }

    /// <summary>
    /// Resets all seen tips for a player.
    /// </summary>
    public async void ResetAllSeenTips(NetUserId player)
    {
        // Clear cache
        _seenTipsCache.Remove(player);

        // Clear database
        await _db.ResetAllSeenTips(player);
    }

    /// <summary>
    /// Checks if a tip has been seen by a player.
    /// </summary>
    public bool HasSeenTip(NetUserId player, ProtoId<TipPrototype> tipId)
    {
        if (!_seenTipsCache.TryGetValue(player, out var seenTips))
            return false;

        return seenTips.Contains(tipId.Id);
    }

    private async void ScheduleTipsForPlayer(ICommonSession session, EntityUid playerEntity)
    {
        // Load seen tips from database if not cached
        if (!_seenTipsCache.ContainsKey(session.UserId))
        {
            var seenTips = await _db.GetSeenTips(session.UserId);
            _seenTipsCache[session.UserId] = seenTips;
        }

        var currentTime = _timing.CurTime;
        var tips = new List<ScheduledTip>();

        foreach (var tipProto in Prototype.EnumeratePrototypes<TipPrototype>())
        {
            // Skip tips the player has already dismissed with "Don't show again"
            if (HasSeenTip(session.UserId, tipProto.ID))
                continue;

            // Check conditions at schedule time
            if (!CheckConditions(playerEntity, session, tipProto))
                continue;

            tips.Add(new ScheduledTip
            {
                TipId = tipProto.ID,
                ShowTime = currentTime + tipProto.Delay,
                PlayerEntity = playerEntity
            });
        }

        if (tips.Count == 0)
            return;

        // Sort by priority then show time
        tips.Sort((a, b) =>
        {
            var protoA = Prototype.Index(a.TipId);
            var protoB = Prototype.Index(b.TipId);
            var priorityCompare = protoA.Priority.CompareTo(protoB.Priority);
            return priorityCompare != 0 ? priorityCompare : a.ShowTime.CompareTo(b.ShowTime);
        });

        _scheduledTips.Remove(session);
        _scheduledTips[session] = tips;
    }

    /// <summary>
    /// Checks if all conditions for a tip are met.
    /// </summary>
    private bool CheckConditions(EntityUid player, ICommonSession session, TipPrototype tip)
    {
        foreach (var condition in tip.Conditions)
        {
            var result = EvaluateCondition(player, session, condition);
            // Apply inversion
            result ^= condition.Invert;

            if (!result)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Evaluates a single condition based on its type.
    /// </summary>
    private bool EvaluateCondition(EntityUid player, ICommonSession session, TipCondition condition)
    {
        return condition switch
        {
            HasCompCondition hasComp => EvaluateHasComp(player, hasComp),
            HasJobCondition hasJob => EvaluateHasJob(player, hasJob),
            HasRoleTypeCondition hasRoleType => EvaluateHasRoleType(player, hasRoleType),
            MinPlaytimeCondition minTracker => EvaluateMinPlaytime(session, minTracker),
            MaxPlaytimeCondition maxTracker => EvaluateMaxPlaytime(session, maxTracker),
            _ => true // Unknown condition types pass by default
        };
    }

    private bool EvaluateHasComp(EntityUid player, HasCompCondition condition)
    {
        if (!_component.TryGetRegistration(condition.Comp, out var registration))
        {
            Log.Warning($"Tip condition references unknown component: {condition.Comp}");
            return false;
        }

        return HasComp(player, registration.Type);
    }

    private bool EvaluateHasJob(EntityUid player, HasJobCondition condition)
    {
        if (!_job.MindTryGetJobId(Mind.GetMind(player), out var jobId))
            return false;

        return jobId == condition.Job;
    }

    private bool EvaluateHasRoleType(EntityUid player, HasRoleTypeCondition condition)
    {
        if (!Mind.TryGetMind(player, out _, out var mind))
            return false;

        foreach (var role in mind.MindRoleContainer.ContainedEntities)
        {
            if (!TryComp<MindRoleComponent>(role, out var mindRole))
                continue;

            if (mindRole.RoleType == condition.RoleType)
                return true;
        }

        return false;
    }

    private bool EvaluateMinPlaytime(ICommonSession session, MinPlaytimeCondition condition)
    {
        if (!_playtime.TryGetTrackerTime(session, condition.Tracker, out var time))
            return false;

        return time.Value >= condition.Time;
    }

    private bool EvaluateMaxPlaytime(ICommonSession session, MaxPlaytimeCondition condition)
    {
        if (!_playtime.TryGetTrackerTime(session, condition.Tracker, out var time))
            return true; // No time tracked = 0 minutes, which is less than any positive max

        return time.Value < condition.Time;
    }

    /// <summary>
    /// Cancels all scheduled tips for a player.
    /// </summary>
    public void CancelTipsForPlayer(ICommonSession session)
    {
        _scheduledTips.Remove(session);
    }
}
