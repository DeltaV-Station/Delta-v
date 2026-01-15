using Content.Server._Impstation.Thaven;
using Content.Shared._DV.EntityEffects.Effects.Thaven;
using Content.Shared._Impstation.Thaven.Components;
using Content.Shared.EntityEffects;

namespace Content.Server._DV.EntityEffects.Effects.Thaven;

public sealed class ThavenRefreshMoodsEffectSystem : EntityEffectSystem<ThavenMoodsComponent, ThavenRefreshMoods>
{
    [Dependency] private readonly ThavenMoodsSystem _moods = default!;

    protected override void Effect(Entity<ThavenMoodsComponent> entity, ref EntityEffectEvent<ThavenRefreshMoods> args)
    {
        _moods.RefreshMoods(entity, entity.Comp);
    }
}
