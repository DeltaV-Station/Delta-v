using Content.Server._DV.Abilities;
using Content.Shared._DV.Light;

namespace Content.Server._DV.Light;

public sealed partial class BreakLightsOnSpawnSystem : EntitySystem
{
    [Dependency] private readonly ShatterLightsAbilitySystem _shatterLights = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BreakLightsOnSpawnComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BreakLightsOnSpawnComponent> entity, ref MapInitEvent args)
    {
        _shatterLights.ShatterLightsAround(entity.Owner, entity.Comp.Radius, entity.Comp.LineOfSight);
    }
}
