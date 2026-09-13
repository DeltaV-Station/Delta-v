using Content.Shared._DV.Physics.Components;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._DV.Physics.Systems;

/// <summary>
/// This takes care of adding and removing Density when the component is added or removed.
/// </summary>
public sealed class ChangeDensityStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly FixtureSystem _fixture = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ChangeDensityStatusEffectComponent, StatusEffectAppliedEvent>(OnEffectApplied);
        SubscribeLocalEvent<ChangeDensityStatusEffectComponent, StatusEffectRemovedEvent>(OnEffectRemoved);
    }

    private void OnEffectApplied(Entity<ChangeDensityStatusEffectComponent> effect, ref StatusEffectAppliedEvent args)
    {
        _fixture.TryCreateFixture(args.Target, new PhysShapeCircle(), effect.Comp.FixtureId, effect.Comp.Amount, false, friction: 0f);
    }

    private void OnEffectRemoved(Entity<ChangeDensityStatusEffectComponent> effect, ref StatusEffectRemovedEvent args)
    {
        _fixture.DestroyFixture(args.Target, effect.Comp.FixtureId);
    }
}
