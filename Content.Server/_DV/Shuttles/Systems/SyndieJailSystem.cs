using Content.Server._DV.Shuttles.Components;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Events;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server._DV.Shuttles.Systems;

public sealed class SyndieJailSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SyndieJailComponent, StationPostInitEvent>(OnStationStartup);
    }

    private void OnStationStartup(Entity<SyndieJailComponent> ent, ref StationPostInitEvent args)
    {
        var cc = Comp<StationCentcommComponent>(ent);
        if (cc.Entity is not {} gridUid || cc.MapEntity is not {} map)
            return;

        var mapId = Comp<MapComponent>(map).MapId;
        var offset = _random.NextVector2(ent.Comp.MinRange, ent.Comp.MaxRange);
        var options = new MapLoadOptions
        {
            Offset = _transform.GetWorldPosition(gridUid) + offset,
            LoadMap = false
        };
        _mapLoader.TryLoad(mapId, ent.Comp.Path.ToString(), out _, options);
    }
}
