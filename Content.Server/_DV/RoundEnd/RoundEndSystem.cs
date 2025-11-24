using Content.Server.Voting.Managers;
using Content.Server.Voting;
using Content.Shared._DV.CCVars;

namespace Content.Server.RoundEnd;

public sealed partial class RoundEndSystem : EntitySystem
{
    [Dependency] IVoteManager _vote = default!;

    public void CallEvacuationVote()
    {
        var options = new VoteOptions
        {
            DisplayVotes = false,
            Title = Loc.GetString("round-end-system-vote-title"),
            Duration = _cfg.GetCVar(DCCVars.EmergencyShuttleVoteTime),
            InitiatorText = Loc.GetString("vote-options-server-initiator-text")
        };

        options.Options.Add((Loc.GetString("round-end-system-vote-end"), true));
        options.Options.Add((Loc.GetString("round-end-system-vote-continue"), false));

        var vote = _vote.CreateVote(options);

        vote.OnFinished += (_, args) =>
        {
            if (args.Winner == null || (bool)args.Winner)
                RequestRoundEnd(null, false, "round-end-system-vote-shuttle-called-announcement");
        };
    }
}
