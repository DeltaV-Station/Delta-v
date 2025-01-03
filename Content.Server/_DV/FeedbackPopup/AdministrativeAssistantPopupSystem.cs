using Content.Shared._DV.FeedbackOverwatch;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Roles.Jobs;

namespace Content.Server._DV.FeedbackPopup;

/// <summary>
///     System to get feedback on the new job!
/// </summary>
public sealed class AdminAssistantPopupSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly SharedFeedbackOverwatchSystem _feedback = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        var allMinds = _mind.GetAliveHumans();
        HashSet<EntityUid> commandMinds = new();

        // Assumes the assistant is still there at the end of the round.
        var roundHadAssistant = false;
        foreach (var mind in allMinds)
        {
            if (!_job.MindTryGetJob(mind, out var jobProto))
                continue;

            if (_job.MindHasJobWithId(mind, "AdministrativeAssistant"))
            {
                _feedback.SendPopupMind(mind, "AdministrativeAssistantPopupSelf");
                roundHadAssistant = true;
                continue;
            }

            // Basically if they are command, send them the popup.
            if (jobProto.RequireAdminNotify)
                commandMinds.Add(mind);
        }

        if (roundHadAssistant)
            foreach (var mind in commandMinds)
                _feedback.SendPopupMind(mind, "AdministrativeAssistantPopupCommand");
    }
}
