using Content.Server._DV.StationEvents.Components;
using Content.Server.Psionics.Glimmer;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Psionics.Glimmer;
using System.Linq;

namespace Content.Server._DV.StationEvents.Events;

public sealed class LockProbersRule : StationEventSystem<LockProbersRuleComponent>
{
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly GlimmerReactiveSystem _glimmerReactiveSystem = default!;

    protected override void Started(EntityUid uid, LockProbersRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        if (!TryGetRandomStation(out var station))
            return;

        var probers = GetProbers(station.Value);
        if (probers.Count == 0)
        {
            Log.Info($"{ToPrettyString(uid):rule} found no probers on station {station}!");
            ForceEndSelf(uid, gameRule);
            return;
        }
        RobustRandom.Shuffle(probers);
        int probersToLock = RobustRandom.Next(1, probers.Count); // Lock a random number of probers, at least one
        foreach (var prober in probers.Take(probersToLock))
        {
            _glimmerReactiveSystem.LockProber(prober);
        }
    }

    private List<EntityUid> GetProbers(EntityUid station)
    {
        var probers = new List<EntityUid>();
        var query = AllEntityQuery<SharedGlimmerReactiveComponent>();
        while (query.MoveNext(out var uid, out var _))
        {
            var xform = Transform(uid);
            if (xform.GridUid == null)
                continue;

            if (_stationSystem.GetOwningStation(xform.GridUid.Value) == station)
                probers.Add(uid);
        }
        return probers;
    }
}
