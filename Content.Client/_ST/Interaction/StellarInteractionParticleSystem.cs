// SPDX-FileCopyrightText: 2026 Janet Blackquill <uhhadd@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Shared._ST.Interaction;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Client._ST.Interaction;

public sealed class StellarInteractionParticleSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;

    private const string AnimateKey = "particle-animation";

    private static readonly Dictionary<StellarInteractionParticleType, EntProtoId> InteractionParticleIds = new ()
    {
        { StellarInteractionParticleType.Use, "StellarInteractionParticleUse" },
        { StellarInteractionParticleType.Pull, "StellarInteractionParticlePull" },
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<StellarInteractionParticleEvent>(OnInteractionParticle);
    }

    private void OnInteractionParticle(StellarInteractionParticleEvent ev)
    {
        var performer = GetEntity(ev.Performer);
        var used = GetEntity(ev.Used);
        var target = GetEntity(ev.Target);

        if (!Exists(performer) || !Exists(target))
            return;

        if (ev.Type == StellarInteractionParticleType.Pull)
        {
            (performer, target) = (target, performer);
        }

        var performerXform = Transform(performer);
        var targetXform = Transform(target);
        if (performerXform.MapID == MapId.Nullspace || targetXform.MapID == MapId.Nullspace)
            return;

        if (performerXform.ParentUid != targetXform.ParentUid)
            return;

        var performerTargetDelta = targetXform.LocalPosition - performerXform.LocalPosition;
        var particle = Spawn(InteractionParticleIds[ev.Type], performerXform.Coordinates);

        if (used is { } usedEntity && Exists(usedEntity) && TryComp<SpriteComponent>(usedEntity, out var usedSprite))
        {
            _sprite.CopySprite((usedEntity, usedSprite), particle);
            // ES START
            _sprite.SetDrawDepth(particle, (int) Shared.DrawDepth.DrawDepth.Effects);
            // ES END
        }

        var spriteColor = Comp<SpriteComponent>(particle).Color;
        var animation = ev.Type switch
        {
            StellarInteractionParticleType.Use => GetUseAnimation(performerTargetDelta, spriteColor),
            StellarInteractionParticleType.Pull => GetPullAnimation(performerTargetDelta, spriteColor),
            _ => throw new ArgumentOutOfRangeException(nameof(ev), $"Interaction particle event has unknown particle type {ev.Type}"),
        };
        _animation.Play(particle, animation, AnimateKey);
    }

    private Animation GetUseAnimation(Vector2 endOffset, Color color)
    {
        var startRotation = _random.NextAngle(Angle.FromDegrees(-40), Angle.FromDegrees(40));
        var endRotation = Angle.Zero;
        var startScale = new Vector2(0.3f, 0.3f);
        var endScale = new Vector2(1f, 1f);
        var rotationLength = TimeSpan.FromMilliseconds(600);

        var startOffset = new Vector2();
        var offsetLength = TimeSpan.FromMilliseconds(250);

        var startColor = color.WithAlpha(color.A * 0.7f);
        var endColor = color.WithAlpha(0f);
        var colorLength = rotationLength + offsetLength;

        return new Animation()
        {
            Length = colorLength,

            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startRotation, 0f),
                        new AnimationTrackProperty.KeyFrame(endRotation, (float)rotationLength.TotalSeconds, Easings.OutBack),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Scale),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startScale, 0f),
                        new AnimationTrackProperty.KeyFrame(endScale, (float)rotationLength.TotalSeconds, Easings.OutBack),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startOffset, 0f),
                        new AnimationTrackProperty.KeyFrame(endOffset, (float)offsetLength.TotalSeconds, Easings.OutBack),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startColor, 0f),
                        new AnimationTrackProperty.KeyFrame(startColor, (float)rotationLength.TotalSeconds),
                        new AnimationTrackProperty.KeyFrame(endColor, (float)offsetLength.TotalSeconds, Easings.InOutCirc),
                    },
                },
            },
        };
    }

    private Animation GetPullAnimation(Vector2 endOffset, Color color)
    {
        var rotationLength = TimeSpan.FromMilliseconds(8f * (1000f / 12f));

        var startOffset = new Vector2();
        var offsetLength = TimeSpan.FromMilliseconds(4f * (1000f / 12f));

        var endColor = color.WithAlpha(0f);

        return new Animation
        {
            Length = rotationLength,

            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startOffset, 0f),
                        new AnimationTrackProperty.KeyFrame(endOffset, (float)rotationLength.TotalSeconds, Easings.InOutCirc),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(color, 0f),
                        new AnimationTrackProperty.KeyFrame(color, (float)offsetLength.TotalSeconds),
                        new AnimationTrackProperty.KeyFrame(endColor, (float)rotationLength.TotalSeconds, Easings.InOutCirc),
                    },
                },
            },
        };
    }
}
