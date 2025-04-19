using Content.Server._DV.Shuttles.Components;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Events;
using Robust.Shared.EntitySerialization.Systems;
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
        {
            Log.Warning($"No centcomm grid to load syndie jail from {ToPrettyString(ent)}!");
            return;
        }

        var mapId = Comp<MapComponent>(map).MapId;
        var offset = _random.NextVector2(ent.Comp.MinRange, ent.Comp.MaxRange);
        _mapLoader.TryLoadGrid(mapId, ent.Comp.Path, out _,
            offset: _transform.GetWorldPosition(gridUid) + offset);
    }
}
