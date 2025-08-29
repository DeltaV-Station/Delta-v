using System.Numerics;
using Content.Shared.Floofstation.Leash.Components;
//using Content.Shared.Paint; // DeltaV - no paint system
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;

namespace Content.Client._Floof.Leash;

public sealed class LeashVisualsOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    private readonly IEntityManager _entMan;
    private readonly SpriteSystem _sprites;
    private readonly SharedTransformSystem _xform;
    private readonly EntityQuery<TransformComponent> _xformQuery;
    private readonly EntityQuery<SpriteComponent> _spriteQuery;
    //private readonly EntityQuery<PaintedComponent> _paintQuery; // DeltaV - no paint system

    public LeashVisualsOverlay(IEntityManager entMan)
    {
        _entMan = entMan;
        _sprites = _entMan.System<SpriteSystem>();
        _xform = _entMan.System<SharedTransformSystem>();
        _xformQuery = _entMan.GetEntityQuery<TransformComponent>();
        _spriteQuery = _entMan.GetEntityQuery<SpriteComponent>();
        //_paintQuery = _entMan.GetEntityQuery<PaintedComponent>(); // DeltaV - no paint system
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var worldHandle = args.WorldHandle;
        worldHandle.SetTransform(Vector2.Zero, Angle.Zero);

        var query = _entMan.EntityQueryEnumerator<LeashedVisualsComponent>();
        while (query.MoveNext(out var visualsComp))
        {
            if (visualsComp.Source is not {Valid: true} source
                || visualsComp.Target is not {Valid: true} target
                || !_xformQuery.TryGetComponent(source, out var xformComp)
                || !_xformQuery.TryGetComponent(target, out var otherXformComp)
                || xformComp.MapID != args.MapId
                || otherXformComp.MapID != xformComp.MapID)
                continue;

            var texture = _sprites.Frame0(visualsComp.Sprite);
            var width = texture.Width / (float) EyeManager.PixelsPerMeter;

            var coordsA = xformComp.Coordinates;
            var coordsB = otherXformComp.Coordinates;

            // If both coordinates are in the same spot (e.g. the leash is being held by the leashed), don't render anything
            if (coordsA.TryDistance(_entMan, _xform, coordsB, out var dist) && dist < 0.01f)
                continue;

            var rotA = xformComp.LocalRotation;
            var rotB = otherXformComp.LocalRotation;
            var offsetA = visualsComp.OffsetSource;
            var offsetB = visualsComp.OffsetTarget;

            // Sprite rotation is the rotation along the Z axis
            // Which is different from transform rotation for all mobs that are seen from the side (instead of top-down)
            if (_spriteQuery.TryGetComponent(source, out var spriteA))
            {
                offsetA *= spriteA.Scale;
                rotA = spriteA.Rotation;
            }
            if (_spriteQuery.TryGetComponent(target, out var spriteB))
            {
                offsetB *= spriteB.Scale;
                rotB = spriteB.Rotation;
            }

            coordsA = coordsA.Offset(rotA.RotateVec(offsetA));
            coordsB = coordsB.Offset(rotB.RotateVec(offsetB));

            var posA = _xform.ToMapCoordinates(coordsA).Position;
            var posB = _xform.ToMapCoordinates(coordsB).Position;
            var diff = (posB - posA);
            var length = diff.Length();

            // So basically, we find the midpoint, then create a box that describes the sprite boundaries, then rotate it
            var midPoint = diff / 2f + posA;
            var angle = (posB - posA).ToWorldAngle();
            var box = new Box2(-width / 2f, -length / 2f, width / 2f, length / 2f);
            var rotate = new Box2Rotated(box.Translated(midPoint), angle, midPoint);

            // Source is always the leash as of now.
            // If it ever changes, make sure to change the visuals comp to include a reference to the leash.
            //var color = _paintQuery.CompOrNull(source)?.Color; // DeltaV - no paint system

            worldHandle.DrawTextureRect(texture, rotate, Color.White); // DeltaV - no paint system, always white
        }
    }
}
