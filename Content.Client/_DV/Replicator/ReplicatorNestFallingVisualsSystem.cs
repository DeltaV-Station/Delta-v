using Content.Shared._DV.Replicator;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Client._DV.Replicator;

public sealed class ReplicatorNestFallingVisualsSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _anim = default!;

    private const string HoleFallingAnimationKey = "hole_fall";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReplicatorNestFallingComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ReplicatorNestFallingComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnComponentInit(Entity<ReplicatorNestFallingComponent> ent, ref ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite) || TerminatingOrDeleted(ent))
            return;

        ent.Comp.OriginalScale = sprite.Scale;
        var animPlayer = EnsureComp<AnimationPlayerComponent>(ent);
        if (_anim.HasRunningAnimation(animPlayer, HoleFallingAnimationKey))
            return;

        _anim.Play((ent, animPlayer), GetFallingAnimation(ent.Comp), HoleFallingAnimationKey);
    }

    private void OnComponentRemove(Entity<ReplicatorNestFallingComponent> ent, ref ComponentRemove args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite) || TerminatingOrDeleted(ent))
            return;

        var animPlayer = EnsureComp<AnimationPlayerComponent>(ent);
        var animEnt = (Entity<AnimationPlayerComponent?>) (ent, animPlayer);
        if (_anim.HasRunningAnimation(animPlayer, HoleFallingAnimationKey))
            _anim.Stop(animEnt, HoleFallingAnimationKey);

        sprite.Scale = ent.Comp.OriginalScale;
    }

    private static Animation GetFallingAnimation(ReplicatorNestFallingComponent component)
    {
        var length = component.AnimationTime;
        return new Animation
        {
            Length = length,
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Scale),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(component.OriginalScale, 0.0f),
                        new AnimationTrackProperty.KeyFrame(component.AnimationScale, length.Seconds),
                    },
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                },
            },
        };
    }
}
