using Content.Client._ES.Lighting.Components;
using Content.Shared._ES.Core.Timer.Components;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Timing;

namespace Content.Client._ES.Lighting;

public sealed class ESTimedDespawnLightFadeSystem : VisualizerSystem<ESTimedDespawnLightFadeComponent>
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;

    private const string FadeTrack = "light-fade";

    protected override void OnAppearanceChange(EntityUid uid, ESTimedDespawnLightFadeComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if (_animationPlayer.HasRunningAnimation(uid, FadeTrack))
            return;

        if (!AppearanceSystem.TryGetData<TimeSpan>(uid, ESTimedDespawnVisuals.DespawnTime, out var time, args.Component) ||
            !TryComp<PointLightComponent>(uid, out var light))
            return;

        var duration = time - _timing.CurTime;

        var animation = new Animation
        {
            Length = duration,
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    Property = nameof(PointLightComponent.Energy),
                    ComponentType = typeof(PointLightComponent),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(light.Energy, 0f),
                        new AnimationTrackProperty.KeyFrame(light.Energy, (float) (duration - component.FadeTime).TotalSeconds),
                        new AnimationTrackProperty.KeyFrame(0f, (float) component.FadeTime.TotalSeconds, Easings.OutSine),
                    }
                }
            }
        };

        _animationPlayer.Play(uid, animation, FadeTrack);
    }
}
