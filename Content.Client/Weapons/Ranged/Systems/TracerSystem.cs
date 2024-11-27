using Content.Shared.Weapons.Ranged.Components;
using Robust.Client.Graphics;
using System.Numerics;
using Content.Client.Weapons.Ranged.Overlays;
using Robust.Shared.Timing;
using Robust.Shared.Map;

namespace Content.Client.Weapons.Ranged.Systems;

public sealed class TracerSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private sealed class TracerData
    {
        public List<Vector2> PositionHistory = new();
        public Color Color;
        public TimeSpan EndTime;
        public float Length;
        public MapId MapId;
    }

    private readonly Dictionary<EntityUid, TracerData> _activeTracers = new();

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new TracerOverlay(this));

        SubscribeLocalEvent<TracerComponent, ComponentStartup>(OnTracerStart);
        SubscribeLocalEvent<TracerComponent, ComponentShutdown>(OnTracerShutdown);
    }

    private void OnTracerStart(EntityUid uid, TracerComponent component, ComponentStartup args)
    {
        var xform = Transform(uid);
        var pos = _transform.GetWorldPosition(xform);

        _activeTracers[uid] = new TracerData
        {
            Color = component.Color,
            EndTime = _timing.CurTime + TimeSpan.FromSeconds(component.Lifetime),
            Length = component.Length,
            PositionHistory = new List<Vector2> { pos },
            MapId = xform.MapID
        };
    }

    private void OnTracerShutdown(EntityUid uid, TracerComponent component, ComponentShutdown args)
    {
        _activeTracers.Remove(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var xformQuery = GetEntityQuery<TransformComponent>();
        var time = _timing.CurTime;
        var toRemove = new List<EntityUid>();

        foreach (var (uid, tracer) in _activeTracers)
        {
            // Remove expired tracers
            if (time > tracer.EndTime)
            {
                toRemove.Add(uid);
                continue;
            }

            // Update position history if entity still exists
            if (!xformQuery.TryGetComponent(uid, out var xform))
            {
                toRemove.Add(uid);
                continue;
            }

            var positions = tracer.PositionHistory;
            var currentPos = _transform.GetWorldPosition(xform);
            positions.Add(currentPos);

            // Maintain history based on desired length
            while (positions.Count > 2 && GetTrailLength(positions) > tracer.Length)
            {
                positions.RemoveAt(0);
            }
        }

        foreach (var uid in toRemove)
        {
            _activeTracers.Remove(uid);
        }
    }

    private static float GetTrailLength(List<Vector2> positions)
    {
        var length = 0f;
        for (var i = 1; i < positions.Count; i++)
        {
            length += Vector2.Distance(positions[i - 1], positions[i]);
        }
        return length;
    }

    public void Draw(DrawingHandleWorld handle, MapId currentMap)
    {
        foreach (var tracer in _activeTracers.Values)
        {
            // Skip if not on current map
            if (tracer.MapId != currentMap)
                continue;

            var positions = tracer.PositionHistory;
            // Draw line segments between all points in history
            for (var i = 1; i < positions.Count; i++)
            {
                handle.DrawLine(positions[i - 1], positions[i], tracer.Color);
            }
        }
    }
}
