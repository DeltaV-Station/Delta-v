using Content.Server._DV.Planet;
using Content.Server._DV.Station.Components;

namespace Content.Server._DV.Station.Systems;

public sealed class StationPlanetSpawnerSystem : EntitySystem
{
    [Dependency] private readonly PlanetSystem _planet = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationPlanetSpawnerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StationPlanetSpawnerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnMapInit(Entity<StationPlanetSpawnerComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.GridPath is not {} path)
            return;

        ent.Comp.Map = _planet.LoadPlanet(ent.Comp.Planet, path);
    }

    private void OnShutdown(Entity<StationPlanetSpawnerComponent> ent, ref ComponentShutdown args)
    {
        QueueDel(ent.Comp.Map);
    }
}
