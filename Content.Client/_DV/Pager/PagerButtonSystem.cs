using Content.Shared._DV.Pager;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Pager;

public sealed class PagerButtonSystem : SharedPagerButtonSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;

    private const string AnimateKey = "pager-press";

    protected override void AfterActivated(Entity<PagerButtonComponent> ent)
    {
        if (!_animationPlayer.HasRunningAnimation(ent.Owner, AnimateKey))
            _animationPlayer.Play(ent.Owner, GetAnimation(ent), AnimateKey);
    }

    private Animation GetAnimation(Entity<PagerButtonComponent> ent)
    {
        const float flickDuration = 20f / 60f;

        return new Animation()
        {
            Length = TimeSpan.FromSeconds(flickDuration),

            AnimationTracks =
            {
                new AnimationTrackSpriteFlick()
                {
                    LayerKey = PagerButtonVisualLayers.Base,
                    KeyFrames =
                    {
                        new AnimationTrackSpriteFlick.KeyFrame(ent.Comp.PressState, 0f),
                        new AnimationTrackSpriteFlick.KeyFrame(ent.Comp.UnpressedState, flickDuration),
                    },
                },
            },
        };
    }
}
