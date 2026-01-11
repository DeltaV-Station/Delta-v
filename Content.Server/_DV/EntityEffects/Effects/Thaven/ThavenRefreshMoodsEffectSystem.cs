using Content.Server._Impstation.Thaven;
using Content.Shared._Impstation.Thaven.Components;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.EntityEffects.Effects.Thaven;

public sealed partial class ThavenRefreshMoodsEffectSystem : EntityEffectSystem<StatusEffectsComponent, ThavenRefreshMoods>
{
    [Dependency] private readonly ThavenMoodsSystem _moods = default!;

    protected override void Effect(Entity<StatusEffectsComponent> entity, ref EntityEffectEvent<ThavenRefreshMoods> args)
    {
        if (!TryComp<ThavenMoodsComponent>(entity, out var moods))
            return;
        
        _moods.RefreshMoods(entity, moods);
    }
}

public sealed partial class ThavenRefreshMoods : EntityEffectBase<ThavenRefreshMoods>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("entity-effect-guidebook-thaven-refresh-moods", ("chance", Probability));
}