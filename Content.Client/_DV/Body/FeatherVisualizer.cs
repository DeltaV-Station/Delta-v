using Content.Shared._DV.Body.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Body;

/// <summary>
/// Colors feathers.
/// </summary>
public sealed class FeatherVisualizer : VisualizerSystem<FeatherComponent>
{
    [Dependency] private readonly ClothingSystem _clothing = default!;

    protected override void OnAppearanceChange(EntityUid uid, FeatherComponent component, ref AppearanceChangeEvent args)
    {
        if (!AppearanceSystem.TryGetData<Color>(uid, FeatherVisuals.BloodColor, out var bloodColor, args.Component) ||
            !AppearanceSystem.TryGetData<Color>(uid, FeatherVisuals.FeatherColor, out var featherColor, args.Component))
        {
            return;
        }

        SpriteSystem.LayerSetColor(uid, FeatherVisualLayers.Feather, featherColor);
        SpriteSystem.LayerSetColor(uid, FeatherVisualLayers.Blood, bloodColor);

        if (TryComp<ClothingComponent>(uid, out var clothing))
        {
            foreach (var slotPair in clothing.ClothingVisuals)
            {
                _clothing.SetLayerColor(clothing, slotPair.Key, "feather", featherColor);
            }
        }
    }
}

public enum FeatherVisualLayers : byte
{
    Feather,
    Blood,
}
