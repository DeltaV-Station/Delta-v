using Content.Server.Silicons.Laws;
using Content.Shared._DV.EntityEffects.Effects.Silicon;
using Content.Shared.EntityEffects;
using Content.Shared.Silicons.Laws.Components;
namespace Content.Server._DV.EntityEffects.Effects.Silicon;

public sealed class IonStormEffectSystem : EntityEffectSystem<IonStormTargetComponent, IonStorm>
{
    [Dependency] private readonly IonStormSystem _ionStorm = default!;

    protected override void Effect(Entity<IonStormTargetComponent> entity, ref EntityEffectEvent<IonStorm> args)
    {
        if (!TryComp<SiliconLawBoundComponent>(entity, out var laws))
            return;

        _ionStorm.IonStormTarget((entity.Owner, laws, entity.Comp));
    }
}
