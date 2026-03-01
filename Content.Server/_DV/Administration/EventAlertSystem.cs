using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.Database;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Destructible;
using Content.Shared.GameTicking;

namespace Content.Server._DV.Administration;

public sealed class EventAlertSystem : EntitySystem
{
    [Dependency] private readonly PlayTimeTrackingManager _playTime = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IEntityManager _entity = default!;


    private static readonly double LateJoinAlertMaxHours = 2.0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
    }

    // Alert when overall playtime is lower than this and player latejoins.
    // Useful for raiders that first-join and then wait in Lobby for a while to slip in or briefly join to get a bit of playtime.
    private void OnSpawnComplete(ref PlayerSpawnCompleteEvent ev)
    {
        var playtimeHours = _playTime.GetOverallPlaytime(ev.Player).TotalHours;
        if (playtimeHours < LateJoinAlertMaxHours && ev.LateJoin)
        {
            _chat.SendAdminAlert($"New player {ev.Player.Name} [{playtimeHours:0.#} hours] joined the round.");

            if (_entity.GetNetEntity(ev.Mob) is { } netEnt &&
                AdminLogManager.CreateTpLinks([(netEnt, ev.Profile.Name)], out var tpLinks))
            {
                _chat.SendAdminAlertNoFormatOrEscape(tpLinks);
            }
        }
    }
}
