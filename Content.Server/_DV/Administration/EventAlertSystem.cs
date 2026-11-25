using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.Destructible;
using Content.Server.GameTicking;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared._DV.CCVars;
using Content.Shared.GameTicking;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Server._DV.Administration;

public sealed class EventAlertSystem : EntitySystem
{
    [Dependency] private readonly PlayTimeTrackingManager _playTime = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private double _lateJoinAlertMaxHours;
    private HashSet<EntityUid> _eorgAlerted = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
        SubscribeLocalEvent<DestructibleComponent, BrokenWithOriginEvent>(OnBrokenWithOrigin);
        SubscribeLocalEvent<TimerTriggerComponent, ActiveTimerTriggerEvent>(OnTimerTrigger);
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
        SubscribeLocalEvent<ActorComponent, AttackedEvent>(OnPlayerAttacked);

        _config.OnValueChanged(DCCVars.LateJoinAlertMaxHours, OnMaxHoursCvarChanged, true); // DeltaV
    }

    private void OnMaxHoursCvarChanged(double hours)
    {
        _lateJoinAlertMaxHours = hours;
    }

    private void OnTimerTrigger(Entity<TimerTriggerComponent> ent, ref ActiveTimerTriggerEvent args)
    {
        if (_gameTicker.RunLevel != GameRunLevel.PostRound || args.User is not { } user)
            return;

        if (_eorgAlerted.Add(user))
        {
            AlertWithLink(
                $"[EORG] {ToPrettyString(user):player} activated timer trigger of {ToPrettyString(ent):item}.",
                user);
        }
    }

    // Alert if player starts destroying stuff at EOR.
    // Only sent if it's the first instance of possible EORG for that player to avoid spam.
    private void OnBrokenWithOrigin(Entity<DestructibleComponent> target, ref BrokenWithOriginEvent ev)
    {
        if (_gameTicker.RunLevel != GameRunLevel.PostRound || !HasComp<ActorComponent>(ev.Origin))
            return;

        if (_eorgAlerted.Add(ev.Origin))
        {
            AlertWithLink($"[EORG] {ToPrettyString(ev.Origin):player} destroyed {ToPrettyString(target):entity}.",
                ev.Origin);
        }
    }

    // Alert if player starts attacking others at EOR.
    // Only sent if it's the first instance of possible EORG for that player to avoid spam.
    private void OnPlayerAttacked(Entity<ActorComponent> targetPlayer, ref AttackedEvent ev)
    {
        if (_gameTicker.RunLevel != GameRunLevel.PostRound)
            return;

        if (_eorgAlerted.Add(ev.User))
        {
            AlertWithLink(
                $"[EORG] {ToPrettyString(ev.User)} attacked {ToPrettyString(targetPlayer):player} using {(ev.Used == ev.User ? "Hands" : ToPrettyString(ev.Used))}.",
                ev.User);
        }
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        _eorgAlerted.Clear();
    }

    // Alert when overall playtime is lower than this and player latejoins.
    // Useful for raiders that first-join and then wait in Lobby for a while to slip in or briefly join to get a bit of playtime.
    private void OnSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        if (!ev.LateJoin)
            return;

        var playtimeHours = _playTime.GetOverallPlaytime(ev.Player).TotalHours;
        if (playtimeHours < _lateJoinAlertMaxHours)
        {
            AlertWithLink($"New player {ev.Player.Name} [{playtimeHours:0.#} hours] joined the round.", ev.Mob);
        }
    }

    public void AlertWithLink(string message, EntityUid playerEnt, MapCoordinates? coords = null)
    {
        _chat.SendAdminAlert(message);
        SendLinks(playerEnt, coords);
    }

    public void SendLinks(EntityUid? playerEnt = null, MapCoordinates? mapCoords = null)
    {
        List<MapCoordinates> coords = new();
        if (playerEnt is not null)
        {
            var originName = "Actor";
            if (TryComp(playerEnt, out MetaDataComponent? meta))
            {
                originName = meta.EntityName;
            }

            if (_entity.GetNetEntity(playerEnt) is { } netEnt &&
                AdminLogManager.CreateTpLinks([(netEnt, originName)], out var tpLinks))
            {
                _chat.SendAdminAlertNoFormatOrEscape(tpLinks);
                coords.Add(_transform.GetMapCoordinates(playerEnt.Value));
            }
        }

        if (mapCoords is not null && mapCoords.Value != MapCoordinates.Nullspace)
        {
            coords.Add(mapCoords.Value);
        }
        if (coords.Count != 0 && AdminLogManager.CreateCordLinks(coords, out var coordLinks))
        {
            _chat.SendAdminAlertNoFormatOrEscape(coordLinks);
        }
    }

    public sealed class BrokenWithOriginEvent : EntityEventArgs
    {
        public readonly EntityUid Origin;

        public BrokenWithOriginEvent(EntityUid origin)
        {
            Origin = origin;
        }
    }
}
