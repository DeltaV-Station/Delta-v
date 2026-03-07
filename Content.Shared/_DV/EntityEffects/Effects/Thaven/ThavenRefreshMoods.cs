using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
namespace Content.Shared._DV.EntityEffects.Effects.Thaven;

public sealed partial class ThavenRefreshMoods : EntityEffectBase<ThavenRefreshMoods>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("entity-effect-guidebook-thaven-refresh-moods", ("chance", Probability));
}
