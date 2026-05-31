using Content.Client._ES.Sprite.Components;
using Content.Shared._ES.Core.Timer.Components;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Timing;

namespace Content.Client._ES.Sprite;

/// <summary>
/// This handles <see cref="ESTimedDespawnSpriteFadeComponent"/>
/// </summary>
public sealed class ESTimedDespawnSpriteFadeSystem : VisualizerSystem<ESTimedDespawnSpriteFadeComponent>
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;

    private const string FadeTrack = "es-sprite-fade";

    protected override void OnAppearanceChange(EntityUid uid, ESTimedDespawnSpriteFadeComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if (args.Sprite is not { } sprite)
            return;

        if (_animationPlayer.HasRunningAnimation(uid, FadeTrack))
            return;

        if (!AppearanceSystem.TryGetData<TimeSpan>(uid, ESTimedDespawnVisuals.DespawnTime, out var time, args.Component))
            return;

        var duration = time - _timing.CurTime;

        var animation = new Animation
        {
            Length = duration,
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    Property = nameof(SpriteComponent.Color),
                    ComponentType = typeof(SpriteComponent),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(sprite.Color, 0f),
                        new AnimationTrackProperty.KeyFrame(sprite.Color, MathF.Max((float) (duration - component.FadeTime).TotalSeconds, 0f)),
                        new AnimationTrackProperty.KeyFrame(sprite.Color.WithAlpha(0f), (float) component.FadeTime.TotalSeconds, Easings.OutSine),
                    },
                },
            },
        };

        _animationPlayer.Play(uid, animation, FadeTrack);
    }
}
