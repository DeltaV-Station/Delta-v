using Content.Server.Fluids.EntitySystems;
using Content.Server.Light.EntitySystems;
using Content.Server.Light.Components;
using Content.Server.Spreader;
using Content.Server.Store.Systems;
using Content.Shared._DV.Silicons;
using Content.Shared.Chemistry.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Light.Components;
using Content.Shared.Maps;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.Server._DV.Silicons;

public sealed class StationAiShopSystem : SharedStationAiShopSystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SpreaderSystem _spreader = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiShopComponent, StationAiShopActionEvent>(OnShopAction);

        SubscribeLocalEvent<StationAiShopComponent, StationAiLightSynthesizerActionEvent>(OnLightSynthesizer);
        SubscribeLocalEvent<StationAiShopComponent, StationAiSmokeActionEvent>(OnEmergencySealant);
    }

    private void OnShopAction(Entity<StationAiShopComponent> ent, ref StationAiShopActionEvent args)
    {
        _store.ToggleUi(args.Performer, ent);
    }

    private void OnLightSynthesizer(Entity<StationAiShopComponent> ent, ref StationAiLightSynthesizerActionEvent args)
    {
        // Grab what light exists on the fixture, delete it. Then add light with respect to fixture.
        var fixture = CompOrNull<PoweredLightComponent>(args.Target);
        if (fixture is null) return;

        var lightProto = fixture.BulbType switch
        {
            LightBulbType.Bulb => args.BulbPrototype,
            LightBulbType.Tube => args.TubePrototype,
            _ => args.BulbPrototype
        };

        if (_poweredLight.EjectBulb(args.Target) is { } oldBulb)
            Del(oldBulb);
        var bulb = Spawn(lightProto);
        if (!_poweredLight.InsertBulb(args.Target, bulb))
        {
            Del(bulb);
            return;
        }

        args.Handled = true;
    }

    private void OnEmergencySealant(Entity<StationAiShopComponent> ent, ref StationAiSmokeActionEvent args)
    {
        var mapCoords = _transform.ToMapCoordinates(args.Target);
        if (!_map.TryFindGridAt(mapCoords, out _, out var grid) ||
            !grid.TryGetTileRef(args.Target, out var tileRef) ||
            tileRef.Tile.IsEmpty)
        {
            return;
        }

        if (_spreader.RequiresFloorToSpread(args.SmokePrototype.ToString()) && tileRef.Tile.IsSpace())
            return;

        var coords = grid.MapToGrid(mapCoords);
        var uid = Spawn(args.SmokePrototype, coords.SnapToGrid());
        _smoke.StartSmoke(uid, args.Solution, args.Duration, args.SpreadAmount);
        args.Handled = true;
    }
}
