using Content.Server._DV.StationEvents.Components;
using Content.Server.Revenant.EntitySystems;
using Content.Server.StationEvents.Events;
using Content.Shared.Chemistry;
using Content.Shared.GameTicking.Components;
using Content.Shared.Item;
using Content.Shared.Magic.Components;
using Content.Shared.Mobs.Components;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Server._DV.StationEvents.Events;

public sealed class RandomAnimationRule : StationEventSystem<RandomAnimationRuleComponent>
{

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly RevenantAnimatedSystem _revenantAnimated = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    protected override void Started(EntityUid uid, RandomAnimationRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var targetList = new List<Entity<AnimateableComponent>>();
        var query = EntityQueryEnumerator<AnimateableComponent>();
        while (query.MoveNext(out var ent, out var animatable))
        {
            if (!HasComp<ItemComponent>(ent) || !_revenantAnimated.CanAnimateObject(ent) || _container.IsEntityInContainer(ent))
                continue;

            targetList.Add((ent, animatable));
        }

        var toAnimate = _random.Next(comp.MinAnimates, comp.MaxAnimates);

        for (var i = 0; i < toAnimate && targetList.Count > 0; i++)
        {
            var animateTarget = _random.Pick(targetList);
            Log.Info("Animating " + animateTarget.Owner);
            _revenantAnimated.TryAnimateObject(animateTarget, TimeSpan.FromSeconds(comp.AnimationTime));
            targetList.Remove(animateTarget);
        }

    }
}
