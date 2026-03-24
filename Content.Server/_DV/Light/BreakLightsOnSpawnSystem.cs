using Content.Shared._DV.Light;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;

namespace Content.Server._DV.Light;

public sealed partial class BreakLightsOnSpawnSystem : EntitySystem
{
    [Dependency] private readonly PsychokineticScreamPowerSystem _psychokineticScream = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BreakLightsOnSpawnComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BreakLightsOnSpawnComponent> entity, ref MapInitEvent args)
    {
        _psychokineticScream.ShatterLightsAround(entity.Owner, entity.Comp.Radius, entity.Comp.LineOfSight);
    }
}
