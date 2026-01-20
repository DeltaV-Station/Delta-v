using System.Numerics;
using Content.Server.Decals;
using Content.Shared._EE.Flight;
using Content.Shared._EE.Footprint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Standing;
using Robust.Shared.Map;

namespace Content.Server._DV.Footprints.EntitySystems;

public sealed partial class FootPrintsSystem : EntitySystem
{

    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly DecalSystem _decalSystem = default!;
    [Dependency] private readonly SharedFlightSystem _flight = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;

    private EntityQuery<FlightComponent> _flightQuery;
    private EntityQuery<TransformComponent> _transformQuery;
    private EntityQuery<StandingStateComponent> _standingQuery;
    private EntityQuery<MobThresholdsComponent> _mobThresholdQuery;

    // CrayonSystem uses a hardcoded offset of [-0.5, -0.5] for clicks
    private static Vector2 _baseDecalOffset = new(-0.5f, -0.5f);

    public override void Initialize()
    {
        base.Initialize();

        _flightQuery = GetEntityQuery<FlightComponent>();
        _transformQuery = GetEntityQuery<TransformComponent>();
        _standingQuery = GetEntityQuery<StandingStateComponent>();
        _mobThresholdQuery = GetEntityQuery<MobThresholdsComponent>();

        SubscribeLocalEvent<FootPrintsComponent, MoveEvent>(OnMove);
    }

    private void OnMove(Entity<FootPrintsComponent> ent, ref MoveEvent args)
    {
        if (ent.Comp.PrintsColor.A <= 0f)
            return;

        if (_flightQuery.TryComp(ent, out var flight) && _flight.IsFlying((ent, flight)))
            return;

        if (!_transformQuery.TryComp(ent, out var transform))
            return;

        if (!_mobThresholdQuery.TryComp(ent, out var mobThresholds))
            return;

        var isCrit = mobThresholds.CurrentThresholdState is MobState.Critical or MobState.Dead;
        var dragging = isCrit || _standingQuery.TryComp(ent, out var standingComp) && _standing.IsDown((ent, standingComp));

        var distance = (transform.LocalPosition - ent.Comp.LastPrintPosition).Length();
        var stepSize = dragging ? ent.Comp.DragSize : ent.Comp.StepSize;

        if (distance <= stepSize)
            return;

        // Need to be on a grid to leave footprints
        if (!_map.TryFindGridAt(_transform.GetMapCoordinates((ent, transform)), out var gridUid, out _))
            return;

        // Calculate spawn coordinates
        var coords = CalcCoords(gridUid, ent.Comp, transform, dragging);
        var angle = dragging
            ? (transform.LocalPosition - ent.Comp.LastPrintPosition).ToAngle() + Angle.FromDegrees(-90f)
            : transform.LocalRotation + Angle.FromDegrees(180f);

        if (!_decalSystem.TryAddDecal(ent.Comp.PrintDecal, coords, out _, color: ent.Comp.PrintsColor, rotation: angle, cleanable: true))
            return;

        ent.Comp.LastPrintPosition = transform.LocalPosition;

        // Alternate feet
        ent.Comp.RightStep = !ent.Comp.RightStep;
        DirtyField(ent.AsNullable(), nameof(FootPrintsComponent.RightStep));

        ent.Comp.PrintsColor = ent.Comp.PrintsColor.WithAlpha(
            Math.Max(0f, ent.Comp.PrintsColor.A - ent.Comp.ColorReduceAlpha));
        DirtyField(ent.AsNullable(), nameof(FootPrintsComponent.PrintsColor));
    }

    private EntityCoordinates CalcCoords(EntityUid gridUid,
        FootPrintsComponent footPrints,
        TransformComponent transform,
        bool dragging)
    {
        // For dragging, place footprint at center
        if (dragging)
            return new EntityCoordinates(gridUid, transform.LocalPosition + _baseDecalOffset);

        // For walking, offset left or right based on which foot
        var footOffset = footPrints.RightStep
            ? new Angle(Angle.FromDegrees(180f) + transform.LocalRotation).RotateVec(footPrints.OffsetPrint)
            : new Angle(transform.LocalRotation).RotateVec(footPrints.OffsetPrint);

        return new EntityCoordinates(gridUid, transform.LocalPosition + _baseDecalOffset + footOffset);
    }
}
