using Content.Server._DV.Station.Components;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Events;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server._DV.Station.Systems;

public sealed partial class StationSecondaryCentcommSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationSecondaryCentcommComponent, StationPostInitEvent>(OnStationPostInit);
    }

    private void OnStationPostInit(Entity<StationSecondaryCentcommComponent> ent, ref StationPostInitEvent args)
    {
        if (!TryComp<StationCentcommComponent>(ent, out var centComm) || centComm.Entity is not { } gridUid || centComm.MapEntity is not { } mapUid)
            return;

        if (!TryComp<MapComponent>(mapUid, out var map))
            return;

        var offset = _random.NextVector2(ent.Comp.MinRange, ent.Comp.MaxRange);
        _mapLoader.TryLoadGrid(map.MapId, ent.Comp.Path, out _, offset: _xform.GetWorldPosition(gridUid) + offset);
    }
}
