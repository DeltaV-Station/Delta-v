using Content.Shared.DeltaV.Abilities.Borgs;
using Robust.Client.GameObjects;

namespace Content.Client.DeltaV.Abilities.Borgs;

/// <summary>
/// Responsible for coloring randomized candy.
/// </summary>
public sealed class RandomizedCandyVisualizer : VisualizerSystem<RandomizedCandyComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, RandomizedCandyComponent component, ref AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite)
            || !AppearanceSystem.TryGetData<Color>(uid, RandomizedCandyVisuals.Color, out var color, args.Component))
        {
            return;
        }

        sprite.LayerSetColor(CandyVisualLayers.Ball, color);
    }
}

public enum CandyVisualLayers : byte
{
    Ball
}
