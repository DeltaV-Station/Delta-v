using Content.Client.Atmos.Components;
using Content.Shared.Atmos;
using Robust.Client.GameObjects;
using Robust.Shared.Map;

namespace Content.Client.Atmos.EntitySystems;

/// <summary>
/// This handles the display of fire effects on flammable entities.
/// </summary>
public sealed class FireVisualizerSystem : VisualizerSystem<FireVisualsComponent>
{
    [Dependency] private readonly PointLightSystem _lights = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FireVisualsComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<FireVisualsComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(EntityUid uid, FireVisualsComponent component, ComponentShutdown args)
    {
        if (component.LightEntity != null)
        {
            Del(component.LightEntity.Value);
            component.LightEntity = null;
        }

        // Need LayerMapTryGet because Init fails if there's no existing sprite / appearancecomp
        // which means in some setups (most frequently no AppearanceComp) the layer never exists.
        if (TryComp<SpriteComponent>(uid, out var sprite) &&
<<<<<<< HEAD
            sprite.LayerMapTryGet(FireVisualLayers.Fire, out var layer))
        {
            sprite.RemoveLayer(layer);
=======
            SpriteSystem.LayerMapTryGet((uid, sprite), FireVisualLayers.Fire, out var layer, false))
        {
            SpriteSystem.RemoveLayer((uid, sprite), layer);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        }
    }

    private void OnComponentInit(EntityUid uid, FireVisualsComponent component, ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || !TryComp(uid, out AppearanceComponent? appearance))
            return;

<<<<<<< HEAD
        sprite.LayerMapReserveBlank(FireVisualLayers.Fire);
        sprite.LayerSetVisible(FireVisualLayers.Fire, false);
        sprite.LayerSetShader(FireVisualLayers.Fire, "unshaded");
        if (component.Sprite != null)
            sprite.LayerSetRSI(FireVisualLayers.Fire, component.Sprite);
=======
        SpriteSystem.LayerMapReserve((uid, sprite), FireVisualLayers.Fire);
        SpriteSystem.LayerSetVisible((uid, sprite), FireVisualLayers.Fire, false);
        sprite.LayerSetShader(FireVisualLayers.Fire, "unshaded");
        if (component.Sprite != null)
            SpriteSystem.LayerSetRsi((uid, sprite), FireVisualLayers.Fire, new ResPath(component.Sprite));
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30

        UpdateAppearance(uid, component, sprite, appearance);
    }

    protected override void OnAppearanceChange(EntityUid uid, FireVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite != null)
            UpdateAppearance(uid, component, args.Sprite, args.Component);
    }

    private void UpdateAppearance(EntityUid uid, FireVisualsComponent component, SpriteComponent sprite, AppearanceComponent appearance)
    {
<<<<<<< HEAD
        if (!sprite.LayerMapTryGet(FireVisualLayers.Fire, out var index))
=======
        if (!SpriteSystem.LayerMapTryGet((uid, sprite), FireVisualLayers.Fire, out var index, false))
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            return;

        AppearanceSystem.TryGetData<bool>(uid, FireVisuals.OnFire, out var onFire, appearance);
        AppearanceSystem.TryGetData<float>(uid, FireVisuals.FireStacks, out var fireStacks, appearance);
<<<<<<< HEAD
        sprite.LayerSetVisible(index, onFire);
=======
        SpriteSystem.LayerSetVisible((uid, sprite), index, onFire);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30

        if (!onFire)
        {
            if (component.LightEntity != null)
            {
                Del(component.LightEntity.Value);
                component.LightEntity = null;
            }

            return;
        }

        if (fireStacks > component.FireStackAlternateState && !string.IsNullOrEmpty(component.AlternateState))
<<<<<<< HEAD
            sprite.LayerSetState(index, component.AlternateState);
        else
            sprite.LayerSetState(index, component.NormalState);
=======
            SpriteSystem.LayerSetRsiState((uid, sprite), index, component.AlternateState);
        else
            SpriteSystem.LayerSetRsiState((uid, sprite), index, component.NormalState);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30

        component.LightEntity ??= Spawn(null, new EntityCoordinates(uid, default));
        var light = EnsureComp<PointLightComponent>(component.LightEntity.Value);

        _lights.SetColor(component.LightEntity.Value, component.LightColor, light);

        // light needs a minimum radius to be visible at all, hence the + 1.5f
        _lights.SetRadius(component.LightEntity.Value, Math.Clamp(1.5f + component.LightRadiusPerStack * fireStacks, 0f, component.MaxLightRadius), light);
        _lights.SetEnergy(component.LightEntity.Value, Math.Clamp(1 + component.LightEnergyPerStack * fireStacks, 0f, component.MaxLightEnergy), light);

        // TODO flickering animation? Or just add a noise mask to the light? But that requires an engine PR.
    }
}

public enum FireVisualLayers : byte
{
    Fire
}
