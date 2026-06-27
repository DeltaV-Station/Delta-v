using System.Numerics;
using Content.Client.Pinpointer.UI;
using Content.Client.Shuttles.UI;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Client._DV.CrewMonitoring;

public sealed class CrewMonitoringShuttleControl : BaseShuttleControl
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    private readonly SharedShuttleSystem _shuttles;
    private readonly SharedTransformSystem _transform;

    public EntityUid? Owner;
    public NavMapControl? NavMap;

    private List<Entity<MapGridComponent>> _grids = new();

    public CrewMonitoringShuttleControl() : base(64f, 256f, 256f)
    {
        _shuttles = EntManager.System<SharedShuttleSystem>();
        _transform = EntManager.System<SharedTransformSystem>();
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        DrawBacking(handle);
        DrawCircles(handle);

        if (Owner is not { } owner || NavMap is not { } navMap)
            return;

        var xformQuery = EntManager.GetEntityQuery<TransformComponent>();
        var fixturesQuery = EntManager.GetEntityQuery<FixturesComponent>();
        var bodyQuery = EntManager.GetEntityQuery<PhysicsComponent>();
        var iffQuery = EntManager.GetEntityQuery<IFFComponent>();

        if (!xformQuery.TryGetComponent(owner, out var xform)
            || xform.MapID == MapId.Nullspace)
        {
            return;
        }

        var coordinates = new EntityCoordinates(owner, Vector2.Zero);

        var mapPos = _transform.ToMapCoordinates(coordinates);
        var ourEntRot = Angle.Zero;
        var ourEntMatrix = Matrix3Helpers.CreateTransform(_transform.GetWorldPosition(xform), ourEntRot);
        Matrix3x2.Invert(ourEntMatrix, out var worldToShuttle);
        var shuttleToView = Matrix3x2.CreateScale(new Vector2(MinimapScale, -MinimapScale)) * Matrix3x2.CreateTranslation(MidPointVector);
        var worldToView = worldToShuttle * shuttleToView;

        var viewBounds = new Box2Rotated(new Box2(-WorldRange, -WorldRange, WorldRange, WorldRange).Translated(mapPos.Position), ourEntRot, mapPos.Position);
        var viewAABB = viewBounds.CalcBoundingBox();

        _grids.Clear();
        _mapManager.FindGridsIntersecting(xform.MapID, new Box2(mapPos.Position - MaxRadarRangeVector, mapPos.Position + MaxRadarRangeVector), ref _grids, approx: true, includeMap: false);

        foreach (var grid in _grids)
        {
            if (!fixturesQuery.HasComponent(grid))
                continue;

            var body = bodyQuery.GetComponent(grid);
            iffQuery.TryComp(grid, out var iff);

            if (!_shuttles.CanDraw(grid, body, iff))
                continue;

            var curGridToWorld = _transform.GetWorldMatrix(grid);
            var curGridToView = curGridToWorld * worldToShuttle * shuttleToView;

            var gridAABB = curGridToWorld.TransformBox(grid.Comp.LocalAABB);
            if (!gridAABB.Intersects(viewAABB))
                continue;

            var labelColor = _shuttles.GetIFFColor(grid, self: false, iff);
            DrawGrid(handle, curGridToView, grid, labelColor);
        }

        var curTime = Timing.RealTime;
        var blinkFrequency = 1f / 1f;
        var lit = curTime.TotalSeconds % blinkFrequency > blinkFrequency / 2f;
        var box = SizeBox;

        foreach (var (coord, value) in navMap.TrackedCoordinates)
        {
            if (!lit || !value.Visible)
                continue;

            var blipPos = _transform.ToMapCoordinates(coord);

            if (mapPos.MapId == MapId.Nullspace)
                continue;

            handle.DrawCircle(Vector2.Clamp(Vector2.Transform(blipPos.Position, worldToView), box.TopLeft, box.BottomRight), float.Sqrt(MinimapScale) * 2f, value.Color);
        }

        foreach (var blip in navMap.TrackedEntities.Values)
        {
            if (blip.Blinks && !lit)
                continue;

            var blipPos = _transform.ToMapCoordinates(blip.Coordinates);

            if (mapPos.MapId == MapId.Nullspace)
                continue;

            handle.DrawCircle(Vector2.Clamp(Vector2.Transform(blipPos.Position, worldToView), box.TopLeft, box.BottomRight), float.Sqrt(MinimapScale) * 2f, blip.Color);
        }
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (Owner is not { } owner || NavMap is null)
            return;

        if (!EntManager.TryGetComponent<TransformComponent>(owner, out var xform) || xform.MapID == MapId.Nullspace)
        {
            return;
        }

        var ourEntRot = Angle.Zero;
        var ourEntMatrix = Matrix3Helpers.CreateTransform(_transform.GetWorldPosition(xform), ourEntRot);
        Matrix3x2.Invert(ourEntMatrix, out var worldToShuttle);
        var shuttleToView = Matrix3x2.CreateScale(new Vector2(MinimapScale, -MinimapScale)) * Matrix3x2.CreateTranslation(MidPointVector);
        var worldToView = worldToShuttle * shuttleToView;
        Matrix3x2.Invert(worldToView, out var viewToWorld);

        if (args.Function == EngineKeyFunctions.UIClick)
        {
            if (NavMap.TrackedEntities.Count == 0)
                return;

            if ((StartDragPosition - args.PointerLocation.Position).Length() > NavMap.MinDragDistance)
                return;

            var localPosition = args.PointerLocation.Position - GlobalPixelPosition;
            var worldPosition = Vector2.Transform(localPosition, viewToWorld);

            var closestEntity = NetEntity.Invalid;
            var closestDistance = float.PositiveInfinity;

            foreach (var (currentEntity, blip) in NavMap.TrackedEntities)
            {
                if (!blip.Selectable)
                    continue;

                var currentDistance = (_transform.ToMapCoordinates(blip.Coordinates).Position - worldPosition).Length();

                if (closestDistance < currentDistance || currentDistance * MinimapScale > NavMap.MaxSelectableDistance)
                    continue;

                closestEntity = currentEntity;
                closestDistance = currentDistance;
            }

            if (closestDistance > NavMap.MaxSelectableDistance || !closestEntity.IsValid())
                return;

            NavMap.TrackEntity(closestEntity);
        }
        else if (args.Function == EngineKeyFunctions.UIRightClick)
        {
            NavMap.TrackEntity(null);
        }
    }
}
