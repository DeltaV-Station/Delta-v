using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.SprayPainter.Prototypes;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.Doors;

public sealed class DoorSystem : SharedDoorSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationSystem = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DoorComponent, AppearanceChangeEvent>(OnAppearanceChange);
        // ES START
        // handle open/close state change on client after animation end, not when server says door is open
        SubscribeLocalEvent<DoorComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    private void OnAnimationCompleted(Entity<DoorComponent> entity, ref AnimationCompletedEvent args)
    {
        if (!TryComp<SpriteComponent>(entity, out var sprite) || args.Key != DoorComponent.OpenCloseKey)
            return;

        switch (entity.Comp.State)
        {
            case DoorState.Open:
                foreach (var (layer, layerState) in entity.Comp.OpenSpriteStates)
                {
                    _sprite.LayerSetRsiState((entity.Owner, sprite), layer, layerState);
                }

                break;
            case DoorState.Closed:
                foreach (var (layer, layerState) in entity.Comp.ClosedSpriteStates)
                {
                    _sprite.LayerSetRsiState((entity.Owner, sprite), layer, layerState);
                }

                break;
        }
    }
    // ES END

    protected override void OnComponentInit(Entity<DoorComponent> ent, ref ComponentInit args)
    {
        var comp = ent.Comp;
        comp.OpenSpriteStates = new List<(DoorVisualLayers, string)>(2);
        comp.ClosedSpriteStates = new List<(DoorVisualLayers, string)>(2);

        comp.OpenSpriteStates.Add((DoorVisualLayers.Base, comp.OpenSpriteState));
        comp.ClosedSpriteStates.Add((DoorVisualLayers.Base, comp.ClosedSpriteState));

        comp.OpeningAnimation = new Animation
        {
            Length = comp.OpeningAnimationTime,
            AnimationTracks =
            {
                new AnimationTrackSpriteFlick
                {
                    LayerKey = DoorVisualLayers.Base,
                    KeyFrames =
                    {
                        new AnimationTrackSpriteFlick.KeyFrame(comp.OpeningSpriteState, 0f),
                    },
                },
            },
        };

        comp.ClosingAnimation = new Animation
        {
            Length = comp.ClosingAnimationTime,
            AnimationTracks =
            {
                new AnimationTrackSpriteFlick
                {
                    LayerKey = DoorVisualLayers.Base,
                    KeyFrames =
                    {
                        new AnimationTrackSpriteFlick.KeyFrame(comp.ClosingSpriteState, 0f),
                    },
                },
            },
        };

        comp.EmaggingAnimation = new Animation
        {
            Length = comp.EmaggingAnimationTime,
            AnimationTracks =
            {
                new AnimationTrackSpriteFlick
                {
                    LayerKey = DoorVisualLayers.BaseEmagging,
                    KeyFrames =
                    {
                        new AnimationTrackSpriteFlick.KeyFrame(comp.EmaggingSpriteState, 0f),
                    },
                },
            },
        };
    }

    private void OnAppearanceChange(Entity<DoorComponent> entity, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData<DoorState>(entity, DoorVisuals.State, out var state, args.Component))
            state = DoorState.Closed;

        if (AppearanceSystem.TryGetData<string>(entity, PaintableVisuals.Prototype, out var prototype, args.Component))
             UpdateSpriteLayers((entity.Owner, args.Sprite), prototype);

        // ES START
        // dont stop all animations for no reason
        //if (_animationSystem.HasRunningAnimation(entity, DoorComponent.AnimationKey))
        //    _animationSystem.Stop(entity.Owner, DoorComponent.AnimationKey);
        // ES END

        // We are checking beforehand since some doors may not have an emagging visual layer, and we don't want LayerSetVisible to throw an error.
        if (_sprite.TryGetLayer(entity.Owner, DoorVisualLayers.BaseEmagging, out var _, false))
            _sprite.LayerSetVisible(entity.Owner, DoorVisualLayers.BaseEmagging, state == DoorState.Emagging);

        UpdateAppearanceForDoorState(entity, args.Sprite, state);
    }

    private void UpdateAppearanceForDoorState(Entity<DoorComponent> entity, SpriteComponent sprite, DoorState state)
    {
        _sprite.SetDrawDepth((entity.Owner, sprite), state is DoorState.Open ? entity.Comp.OpenDrawDepth : entity.Comp.ClosedDrawDepth);

        switch (state)
        {
            case DoorState.Open:
                // ES START
                // If we are already animating the close just let that do its job
                if (_animationSystem.HasRunningAnimation(entity, DoorComponent.OpenCloseKey))
                    return;
                // ES END

                foreach (var (layer, layerState) in entity.Comp.OpenSpriteStates)
                {
                    _sprite.LayerSetRsiState((entity.Owner, sprite), layer, layerState);
                }

                return;
            case DoorState.Closed:
                // ES START
                // If we are already animating the close just let that do its job
                if (_animationSystem.HasRunningAnimation(entity, DoorComponent.OpenCloseKey))
                    return;
                // ES END

                foreach (var (layer, layerState) in entity.Comp.ClosedSpriteStates)
                {
                    _sprite.LayerSetRsiState((entity.Owner, sprite), layer, layerState);
                }

                return;
            case DoorState.Opening:
                if (entity.Comp.OpeningAnimationTime == TimeSpan.Zero)
                    return;

                // ES START
                // since we dont stop them earlier we check here
                if (_animationSystem.HasRunningAnimation(entity, DoorComponent.OpenCloseKey))
                    return;
                // ES END

                _animationSystem.Play(entity, (Animation)entity.Comp.OpeningAnimation, DoorComponent.OpenCloseKey);

                return;
            case DoorState.Closing:
                if (entity.Comp.ClosingAnimationTime == TimeSpan.Zero || entity.Comp.CurrentlyCrushing.Count != 0)
                    return;

                // ES START
                // since we dont stop them earlier we check here
                if (_animationSystem.HasRunningAnimation(entity, DoorComponent.OpenCloseKey))
                    return;
                // ES END

                _animationSystem.Play(entity, (Animation)entity.Comp.ClosingAnimation, DoorComponent.OpenCloseKey);

                return;
            case DoorState.Denying:
                // ES START
                // AnimationKey -> DenyKey
                if (_animationSystem.HasRunningAnimation(entity, DoorComponent.DenyKey))
                    return;

                _animationSystem.Play(entity, (Animation)entity.Comp.DenyingAnimation, DoorComponent.DenyKey);
                // ES END

                return;
            case DoorState.Emagging:
                // ES START
                // AnimationKey -> DenyKey
                if (_animationSystem.HasRunningAnimation(entity, DoorComponent.EmagKey))
                    return;

                if (_sprite.TryGetLayer(entity.Owner, DoorVisualLayers.BaseEmagging, out var _, false))
                    _animationSystem.Play(entity, (Animation)entity.Comp.EmaggingAnimation, DoorComponent.EmagKey);
                // ES END

                return;
        }
    }

    private void UpdateSpriteLayers(Entity<SpriteComponent> sprite, string targetProto)
    {
        if (!_prototypeManager.Resolve(targetProto, out var target))
            return;

        if (!target.TryGetComponent(out SpriteComponent? targetSprite, _componentFactory))
            return;

        _sprite.SetBaseRsi(sprite.AsNullable(), targetSprite.BaseRSI);
    }
}
