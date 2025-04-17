using Content.Server._DV.Objectives.Components;
using Content.Shared._DV.FeedbackOverwatch;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Roles;
using Content.Server.Roles;

namespace Content.Server._DV.FeedbackPopup;

/// <summary>
///     System to get feedback on the new objective!
/// </summary>
public sealed class NukeHostageFeedbackPopupSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedFeedbackOverwatchSystem _feedback = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        if (!IsHostageOps())
            return;

        var allMinds = _mind.GetAliveHumans();

        foreach (var mind in allMinds)
        {
            if (mind.Comp.OwnedEntity != null && _role.MindHasRole<NukeopsRoleComponent>(mind))
                _feedback.SendPopupMind(mind, "NukieHostageRoundEndPopup");
            else
                _feedback.SendPopupMind(mind, "NukieHostageRoundEndCrewPopup");
        }
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || !_mind.TryGetMind(args.Target, out var mindUid, out _) || !IsHostageOps())
            return;

        if (_role.MindHasRole<NukeopsRoleComponent>(mindUid))
            _feedback.SendPopup(args.Target, "NukieHostageRoundEndPopup");
    }


    /// <remarks>
    ///     If even one person has the kidnap heads objective this will return true.
    /// </remarks>
    private bool IsHostageOps()
    {
        return EntityQueryEnumerator<KidnapHeadsConditionComponent>().MoveNext(out _);
    }
}
