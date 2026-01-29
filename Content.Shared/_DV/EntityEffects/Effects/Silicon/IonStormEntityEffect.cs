using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
namespace Content.Shared._DV.EntityEffects.Effects.Silicon;

public sealed partial class IonStorm : EntityEffectBase<IonStorm>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-ion-storm", ("chance", Probability));
}
