using Content.Server.Silicons.Laws;
using Content.Shared.EntityEffects;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
namespace Content.Server._DV.EntityEffects.Effects.Silicon;

public sealed class IonStormEffectSystem : EntityEffectSystem<StatusEffectsComponent, IonStorm>
{
    [Dependency] private readonly IonStormSystem _ionStorm = default!;

    protected override void Effect(Entity<StatusEffectsComponent> entity, ref EntityEffectEvent<IonStorm> args)
    {
        if (!TryComp<SiliconLawBoundComponent>(entity, out var laws))
            return;
        
        if (!TryComp<IonStormTargetComponent>(entity, out var target))
            return;
        
        _ionStorm.IonStormTarget((entity.Owner, laws, target));
    }
}

public sealed partial class IonStorm : EntityEffectBase<IonStorm>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-ion-storm", ("chance", Probability));
}