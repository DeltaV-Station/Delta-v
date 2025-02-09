using Content.Shared._DV.CustomObjectiveSummery;
using Content.Shared.Mind;
using Robust.Shared.Network;

namespace Content.Server._DV.CustomObjectiveSummery;

public sealed class CustomObjectiveSummerySystem : EntitySystem
{
    [Dependency] private readonly IServerNetManager _netManager = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<EvacShuttleLeftEvent>(OnEvacShuttleLeft);

        _netManager.RegisterNetMessage<CustomObjectiveClientSetObjective>(OnCustomObjectiveFeedback);
    }

    private void OnCustomObjectiveFeedback(CustomObjectiveClientSetObjective msg)
    {
        if (!_mind.TryGetMind(msg.MsgChannel.UserId, out var mind))
            return;

        EnsureComp<CustomObjectiveSummeryComponent>(mind.Value, out var comp);

        comp.ObjectiveSummery = msg.Summery;
        Dirty(mind.Value.Owner, comp);
    }

    private void OnEvacShuttleLeft(EvacShuttleLeftEvent args)
    {
        var allMinds = _mind.GetAliveHumans();

        // Assumes the assistant is still there at the end of the round.
        foreach (var mind in allMinds)
        {
            // Only send the popup to people with objectives.
            if (mind.Comp.Objectives.Count == 0)
                continue;

            if (!_mind.TryGetSession(mind, out var session))
                continue;

            RaiseNetworkEvent(new CustomObjectiveSummeryOpenMessage(), session);
        }
    }
}
