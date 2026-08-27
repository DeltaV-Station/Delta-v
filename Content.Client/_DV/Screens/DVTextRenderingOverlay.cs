using System.Linq;
using System.Numerics;
using System.Threading;
using JetBrains.Annotations;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;

namespace Content.Client._DV.Screens;

[UsedImplicitly]
public sealed class DVTextRenderingOverlay : Overlay
{
    [Dependency] private readonly IClyde _clyde = default!;
    private readonly SpriteSystem _sprite;
    private readonly AnimationPlayerSystem _animationPlayer;

    public override OverlaySpace Space => OverlaySpace.ScreenSpaceBelowWorld;

    private readonly Queue<(Entity<DVTextVisualsComponent> Entity, Font Font, CancellationToken Cancellation)> _queue = new();

    public const string MarqueeKey = "dv-text-screen-marquee";

    public DVTextRenderingOverlay(SpriteSystem sprite, AnimationPlayerSystem animationPlayer)
    {
        IoCManager.InjectDependencies(this);
        _sprite = sprite;
        _animationPlayer = animationPlayer;

        ZIndex = -100; // this needs to render before almost everything
    }

    public CancellationTokenSource QueueRender(
        Entity<DVTextVisualsComponent> ent,
        Font font)
    {
        var source = new CancellationTokenSource();
        _queue.Enqueue((ent, font, source.Token));

        return source;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var screenHandle = args.ScreenHandle;

        while (_queue.TryDequeue(out var queued))
        {
            if (queued.Cancellation.IsCancellationRequested)
                continue;

            var font = queued.Font;
            foreach (var row in queued.Entity.Comp.Rows)
            {
                if (row.Text == string.Empty)
                {
                    _sprite.LayerSetTexture(queued.Entity.Owner, row.Layer, null);
                    continue;
                }

                var dimensions = screenHandle.GetDimensions(queued.Font, row.Text, 1f);
                var dimensionsInt = new Vector2i((int)MathF.Round(dimensions.X), (int)MathF.Round(dimensions.Y));

                if (row.Texture is null || row.Texture.Size != dimensionsInt)
                {
                    row.Texture = _clyde.CreateRenderTarget(dimensionsInt,
                        new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8),
                        name: $"dv-text-visuals-{queued.Entity.Owner.Id}");

                    _sprite.LayerSetTexture(queued.Entity.Owner, row.Layer, row.Texture.Texture);
                    _sprite.LayerSetOffset(queued.Entity.Owner, row.Layer, row.Offset);
                }

                args.DrawingHandle.RenderInRenderTarget(row.Texture,
                    () =>
                    {
                        screenHandle.DrawString(font, Vector2.Zero, row.Text);
                    },
                    Color.Transparent);
            }

            _animationPlayer.Stop(queued.Entity.Owner, MarqueeKey);
            if (CreateMarqueeAnimation(queued.Entity) is { } animation)
            {
                queued.Entity.Comp.Animation = animation;
                _animationPlayer.Play(queued.Entity.Owner, animation, MarqueeKey);
            }
        }
    }

    private Animation? CreateMarqueeAnimation(Entity<DVTextVisualsComponent> ent)
    {
        var largestRowWidth = ent.Comp.Rows.Aggregate(0, (i, row) => Math.Max(i, row.Texture?.Size.X ?? 0));
        var animationTime = ent.Comp.MarqueeRate * largestRowWidth;
        var marqueeWidth = new Vector2((float)ent.Comp.MarqueeWidth / EyeManager.PixelsPerMeter, 0);

        var animation = new Animation
        {
            Length = animationTime,
        };

        foreach (var row in ent.Comp.Rows)
        {
            if (row.Texture is null)
                continue;

            var rowHalfWidth = new Vector2(row.Texture.Size.X / 2f / EyeManager.PixelsPerMeter, 0f);

            if (row.Texture.Size.X <= ent.Comp.MarqueeWidth)
                continue;

            animation.AnimationTracks.Add(new AnimationTrackLayerOffset()
            {
                Layer = row.Layer,
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(row.Offset + rowHalfWidth + marqueeWidth, 0f),
                    new AnimationTrackProperty.KeyFrame(row.Offset - rowHalfWidth - marqueeWidth, (float)animationTime.TotalSeconds),
                },
            });
        }

        return animation.AnimationTracks.Count > 0 ? animation : null;
    }

    public sealed class AnimationTrackLayerOffset : AnimationTrackProperty
    {
        public required Enum Layer;
        private readonly SpriteSystem _sprite = IoCManager.Resolve<IEntityManager>().System<SpriteSystem>();

        protected override void ApplyProperty(object context, object value)
        {
            if (value is not Vector2 vector)
                throw new InvalidOperationException("Value must be a .");

            _sprite.LayerSetOffset((EntityUid) context, Layer, vector);
        }
    }
}
