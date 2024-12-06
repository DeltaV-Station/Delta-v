using Content.Server.DeltaV.Planet;
using Content.Server.DeltaV.Station.Components;

namespace Content.Server.DeltaV.Station.Systems;

public sealed class StationPlanetSpawnerSystem : EntitySystem
{
    [Dependency] private readonly PlanetSystem _planet = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationPlanetSpawnerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<StationPlanetSpawnerComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.GridPath is not {} path)
            return;

        ent.Comp.Map = _planet.LoadPlanet(ent.Comp.Planet, path.ToString());
    }
}
