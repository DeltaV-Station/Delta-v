using Content.Client.Light.Components;
using Content.Shared.Light.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Client.Light.EntitySystems;

public sealed class ExpendableLightSystem : VisualizerSystem<ExpendableLightComponent>
{
    [Dependency] private readonly PointLightSystem _pointLightSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly LightBehaviorSystem _lightBehavior = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExpendableLightComponent, ComponentShutdown>(OnLightShutdown);
    }

    private void OnLightShutdown(EntityUid uid, ExpendableLightComponent component, ComponentShutdown args)
    {
        component.PlayingStream = _audioSystem.Stop(component.PlayingStream);
    }

    protected override void OnAppearanceChange(EntityUid uid, ExpendableLightComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<string>(uid, ExpendableLightVisuals.Behavior, out var lightBehaviourID, args.Component)
        &&  TryComp<LightBehaviourComponent>(uid, out var lightBehaviour))
        {
            _lightBehavior.StopLightBehaviour((uid, lightBehaviour));

            if (!string.IsNullOrEmpty(lightBehaviourID))
            {
                _lightBehavior.StartLightBehaviour((uid, lightBehaviour), lightBehaviourID);
            }
            else if (TryComp<PointLightComponent>(uid, out var light))
            {
                _pointLightSystem.SetEnabled(uid, false, light);
            }
        }

        if (!AppearanceSystem.TryGetData<ExpendableLightState>(uid, ExpendableLightVisuals.State, out var state, args.Component))
            return;

        switch (state)
        {
            case ExpendableLightState.Lit:
                _audioSystem.Stop(comp.PlayingStream);
                comp.PlayingStream = _audioSystem.PlayPvs(
                    comp.LoopedSound, uid)?.Entity;

<<<<<<< HEAD
                if (args.Sprite.LayerMapTryGet(ExpendableLightVisualLayers.Overlay, out var layerIdx, true))
                {
                    if (!string.IsNullOrWhiteSpace(comp.IconStateLit))
                        args.Sprite.LayerSetState(layerIdx, comp.IconStateLit);
=======
                if (SpriteSystem.LayerMapTryGet((uid, args.Sprite), ExpendableLightVisualLayers.Overlay, out var layerIdx, true))
                {
                    if (!string.IsNullOrWhiteSpace(comp.IconStateLit))
                        SpriteSystem.LayerSetRsiState((uid, args.Sprite), layerIdx, comp.IconStateLit);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
                    if (!string.IsNullOrWhiteSpace(comp.SpriteShaderLit))
                        args.Sprite.LayerSetShader(layerIdx, comp.SpriteShaderLit);
                    else
                        args.Sprite.LayerSetShader(layerIdx, null, null);
                    if (comp.GlowColorLit.HasValue)
<<<<<<< HEAD
                        args.Sprite.LayerSetColor(layerIdx, comp.GlowColorLit.Value);
                    args.Sprite.LayerSetVisible(layerIdx, true);
                }

                if (comp.GlowColorLit.HasValue)
                    args.Sprite.LayerSetColor(ExpendableLightVisualLayers.Glow, comp.GlowColorLit.Value);
                args.Sprite.LayerSetVisible(ExpendableLightVisualLayers.Glow, true);
=======
                        SpriteSystem.LayerSetColor((uid, args.Sprite), layerIdx, comp.GlowColorLit.Value);
                    SpriteSystem.LayerSetVisible((uid, args.Sprite), layerIdx, true);
                }

                if (comp.GlowColorLit.HasValue)
                    SpriteSystem.LayerSetColor((uid, args.Sprite), ExpendableLightVisualLayers.Glow, comp.GlowColorLit.Value);
                SpriteSystem.LayerSetVisible((uid, args.Sprite), ExpendableLightVisualLayers.Glow, true);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30

                break;
            case ExpendableLightState.Dead:
                comp.PlayingStream = _audioSystem.Stop(comp.PlayingStream);
<<<<<<< HEAD
                if (args.Sprite.LayerMapTryGet(ExpendableLightVisualLayers.Overlay, out layerIdx, true))
                {
                    if (!string.IsNullOrWhiteSpace(comp.IconStateSpent))
                        args.Sprite.LayerSetState(layerIdx, comp.IconStateSpent);
=======
                if (SpriteSystem.LayerMapTryGet((uid, args.Sprite), ExpendableLightVisualLayers.Overlay, out layerIdx, true))
                {
                    if (!string.IsNullOrWhiteSpace(comp.IconStateSpent))
                        SpriteSystem.LayerSetRsiState((uid, args.Sprite), layerIdx, comp.IconStateSpent);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
                    if (!string.IsNullOrWhiteSpace(comp.SpriteShaderSpent))
                        args.Sprite.LayerSetShader(layerIdx, comp.SpriteShaderSpent);
                    else
                        args.Sprite.LayerSetShader(layerIdx, null, null);
                }

<<<<<<< HEAD
                args.Sprite.LayerSetVisible(ExpendableLightVisualLayers.Glow, false);
=======
                SpriteSystem.LayerSetVisible((uid, args.Sprite), ExpendableLightVisualLayers.Glow, false);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
                break;
        }
    }
}
