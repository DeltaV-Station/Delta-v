using Content.Server._DV.Planet;
using Content.Server.GameTicking.Events;
using Content.Shared._DV.Planet;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._DV.AkashicFold;

// entity component system? more like entity nothing system amirite
// stealing coscult code basically
// todo: delete these comments so i don't seem like a complete nerd
public sealed class AkashicFoldSystem : EntitySystem
{
    [Dependency] private readonly PlanetSystem _planet = default!;

    public static readonly ProtoId<PlanetPrototype> FoldPlanet = "AkashicFold";
    private readonly ResPath _baseGridPath = new("Maps/_DV/Nonstations/glacier_surface_outpost.yml");
    public static EntityUid? Map;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);

        //SubscribeLocalEvent<AkashicFoldSpawnerComponent, MapInitEvent>(OnMapInit);
        //SubscribeLocalEvent<AkashicFoldSpawnerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        Map = _planet.LoadPlanet(FoldPlanet, _baseGridPath);
    }
}
