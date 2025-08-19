using Content.Client.Storage.Components;
using Content.Shared.Rounding;
using Content.Shared.Storage;
using Robust.Client.GameObjects;

namespace Content.Client.Storage.Systems;

/// <inheritdoc cref="StorageContainerVisualsComponent"/>
public sealed class StorageContainerVisualsSystem : VisualizerSystem<StorageContainerVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, StorageContainerVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData<int>(uid, StorageVisuals.StorageUsed, out var used, args.Component))
            return;

        if (!AppearanceSystem.TryGetData<int>(uid, StorageVisuals.Capacity, out var capacity, args.Component))
            return;

        var fraction = used / (float) capacity;

<<<<<<< HEAD
        if (!args.Sprite.LayerMapTryGet(component.FillLayer, out var fillLayer))
=======
        if (!SpriteSystem.LayerMapTryGet((uid, args.Sprite), component.FillLayer, out var fillLayer, false))
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            return;

        var closestFillSprite = Math.Min(ContentHelpers.RoundToNearestLevels(fraction, 1, component.MaxFillLevels + 1),
            component.MaxFillLevels);

        if (closestFillSprite > 0)
        {
            if (component.FillBaseName == null)
                return;

<<<<<<< HEAD
            args.Sprite.LayerSetVisible(fillLayer, true);
            var stateName = component.FillBaseName + closestFillSprite;
            args.Sprite.LayerSetState(fillLayer, stateName);
        }
        else
        {
            args.Sprite.LayerSetVisible(fillLayer, false);
=======
            SpriteSystem.LayerSetVisible((uid, args.Sprite), fillLayer, true);
            var stateName = component.FillBaseName + closestFillSprite;
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), fillLayer, stateName);
        }
        else
        {
            SpriteSystem.LayerSetVisible((uid, args.Sprite), fillLayer, false);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        }
    }
}
