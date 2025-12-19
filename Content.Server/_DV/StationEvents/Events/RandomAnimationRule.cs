using Content.Server._DV.StationEvents.Components;
using Content.Server.Revenant.EntitySystems;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Item;
using Content.Shared.Magic.Components;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Server._DV.StationEvents.Events;

public sealed class RandomAnimationRule : StationEventSystem<RandomAnimationRuleComponent>
{

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly RevenantAnimatedSystem _revenantAnimated = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private EntityQuery<ItemComponent> _itemQuery;

    public override void Initialize()
    {
        base.Initialize();

        _itemQuery = GetEntityQuery<ItemComponent>();
    }

    protected override void Started(EntityUid uid, RandomAnimationRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        if (!TryGetRandomStation(out var station))
            return;

        var targetList = new List<Entity<AnimateableComponent>>();
        var query = EntityQueryEnumerator<AnimateableComponent>();
        while (query.MoveNext(out var ent, out var animateable))
        {
            if (StationSystem.GetOwningStation(ent) != station)
                continue;

            if (!_itemQuery.HasComp(ent) || !_revenantAnimated.CanAnimateObject(ent) || _container.IsEntityInContainer(ent))
                continue;

            targetList.Add((ent, animateable));
        }

        var toAnimate = _random.Next(comp.MinAnimates, comp.MaxAnimates + 1);

        for (var i = 0; i < toAnimate && targetList.Count > 0; i++)
        {
            var animateTarget = _random.PickAndTake(targetList);
            _revenantAnimated.TryAnimateObject(animateTarget, TimeSpan.FromSeconds(_random.NextFloat(comp.MinTime, comp.MaxTime)));
        }
    }
}
