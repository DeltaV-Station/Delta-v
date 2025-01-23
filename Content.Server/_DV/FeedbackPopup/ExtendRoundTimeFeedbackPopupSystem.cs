using Content.Server.GameTicking;
using Content.Shared._DV.FeedbackOverwatch;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Robust.Shared.Timing;

namespace Content.Server._DV.FeedbackPopup;

public sealed partial class ExtendRoundTimeFeedbackPopupSystem : EntitySystem
{

    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedFeedbackOverwatchSystem _feedback = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        if (_gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan) <= TimeSpan.FromHours(2))
            return;

        var allMinds = _mind.GetAliveHumans();

        foreach (var mind in allMinds)
            _feedback.SendPopupMind(mind, "RoundTimePopup");
    }
}
