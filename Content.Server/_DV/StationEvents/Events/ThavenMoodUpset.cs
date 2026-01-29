using Content.Server._DV.StationEvents.Components;
using Content.Server._Impstation.Thaven;
using Content.Server.StationEvents.Events;
using Content.Shared._Impstation.Thaven.Components;
using Content.Shared.GameTicking.Components;

namespace Content.Server._DV.StationEvents.Events;

public sealed class ThavenMoodUpset : StationEventSystem<ThavenMoodUpsetRuleComponent>
{
    [Dependency] private readonly ThavenMoodsSystem _thavenMoods = default!;

    protected override void Started(EntityUid uid, ThavenMoodUpsetRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        if (comp.NewSharedMoods)
        {
            _thavenMoods.NewSharedMoods();
        }

        var thavens = EntityQueryEnumerator<ThavenMoodsComponent>();
        while (thavens.MoveNext(out var thavenUid, out var thavenComp))
        {
            if(comp.RefreshPersonalMoods)
                _thavenMoods.RefreshMoods(thavenUid, thavenComp);

            if(comp.AddWildcardMood)
                _thavenMoods.AddWildcardMood((thavenUid, thavenComp));
        }
    }
}
