using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Anomaly.Components;

namespace Content.Server._DV.Objectives.Systems;

public sealed class CritAnomalyConditionSystem : EntitySystem
{
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnomalyShutdownEvent>(OnAnomalyShutdown);
    }

    private void OnAnomalyShutdown(ref AnomalyShutdownEvent args)
    {
        // don't care about decaying
        if (!args.Supercritical)
            return;

        // everyone with the objective succeeds because you could trick someone to crit it for you
        // without ever touching an ape yourself
        var query = EntityQueryEnumerator<CritAnomalyConditionComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            _codeCondition.SetCompleted(uid);
        }
    }
}
