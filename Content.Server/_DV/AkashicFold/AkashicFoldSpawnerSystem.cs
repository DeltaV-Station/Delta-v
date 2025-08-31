using Content.Server._DV.AkashicFold.Components;
using Content.Server._DV.Planet;

namespace Content.Server._DV.AkashicFold;

// okay look this is entirely copy-pasted from StationPlanetSpawnerSystem but every class is sealed so :shrug: what am i supposed to do
// i am doing this as more of a proof-of-concept i will ask smarter people then me the Correct way to do this later
// todo: delete these comments so i don't seem like a complete nerd
public sealed class AkashicFoldSpawnerSystem : EntitySystem
{
    [Dependency] private readonly PlanetSystem _planet = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AkashicFoldSpawnerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AkashicFoldSpawnerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnMapInit(Entity<AkashicFoldSpawnerComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.GridPath is not {} path)
            return;

        ent.Comp.Map = _planet.LoadPlanet(ent.Comp.Planet, path);
    }

    private void OnShutdown(Entity<AkashicFoldSpawnerComponent> ent, ref ComponentShutdown args)
    {
        QueueDel(ent.Comp.Map);
    }
}
