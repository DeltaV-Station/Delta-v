using Content.Shared._DV.FeedbackOverwatch;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.NukeOps;

namespace Content.Server._DV.FeedbackPopup;

/// <summary>
///     System to get feedback on the new objective!
/// </summary>
public sealed class NukeHostageFeedbackPopupSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedFeedbackOverwatchSystem _feedback = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        var allMinds = _mind.GetAliveHumans();

        foreach (var mind in allMinds)
        {
            if (mind.Comp.OwnedEntity != null && HasComp<NukeOperativeComponent>(mind.Comp.OwnedEntity))
                _feedback.SendPopupMind(mind, "NukieHostageRoundEndPopup");
            else
                _feedback.SendPopupMind(mind, "NukieHostageRoundEndCrewPopup");
        }
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (HasComp<NukeOperativeComponent>(args.Target))
            _feedback.SendPopup(args.Target, "NukieHostageRoundEndPopup");
    }
}
