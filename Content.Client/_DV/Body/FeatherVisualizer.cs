using Content.Shared._DV.Body.Components;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Body;

/// <summary>
/// Colors feathers.
/// </summary>
public sealed class FeatherVisualizer : VisualizerSystem<FeatherComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, FeatherComponent component, ref AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (AppearanceSystem.TryGetData<Color>(uid, FeatherVisuals.FeatherColor, out var featherColor, args.Component))
        {
            SpriteSystem.LayerSetColor(uid, FeatherVisualLayers.Feather, featherColor);
        }

        if (!AppearanceSystem.TryGetData<Color>(uid, FeatherVisuals.BloodColor, out var bloodColor, args.Component))
            return;

        SpriteSystem.LayerSetColor(uid, FeatherVisualLayers.Blood, bloodColor);
    }
}

public enum FeatherVisualLayers : byte
{
    Feather,
    Blood,
}
