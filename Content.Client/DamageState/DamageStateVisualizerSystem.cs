using Content.Shared.Mobs;
using Robust.Client.GameObjects;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client.DamageState;

public sealed class DamageStateVisualizerSystem : VisualizerSystem<DamageStateVisualsComponent>
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    protected override void OnAppearanceChange(EntityUid uid, DamageStateVisualsComponent component, ref AppearanceChangeEvent args)
    {
        var sprite = args.Sprite;

        if (sprite == null || !AppearanceSystem.TryGetData<MobState>(uid, MobStateVisuals.State, out var data, args.Component))
        {
            return;
        }

        if (!component.States.TryGetValue(data, out var layers))
        {
            return;
        }

        // Brain no worky rn so this was just easier.
        foreach (var key in new[] { DamageStateVisualLayers.Base, DamageStateVisualLayers.BaseUnshaded })
        {
            if (!_sprite.LayerMapTryGet((uid, sprite), key, out _, false)) continue;

            _sprite.LayerSetVisible((uid, sprite), key, false);
        }

        foreach (var (key, state) in layers)
        {
            // Inheritance moment.
            if (!_sprite.LayerMapTryGet((uid, sprite), key, out _, false)) continue;

            _sprite.LayerSetVisible((uid, sprite), key, true);
            _sprite.LayerSetRsiState((uid, sprite), key, state);
        }

        // Begin DeltaV Additions - Hideable Layers
        var toShow = new List<string>(component.HiddenLayers);
        component.HiddenLayers.Clear();
        if (component.ToHide.TryGetValue(data, out var toHide))
        {
            foreach (var key in toHide)
            {
                toShow.Remove(key);
                if (component.HiddenLayers.Contains(key))
                    continue; // Already hidden, nothing else to do

                // Hide the specified layer and store it for later
                _sprite.LayerSetVisible((uid, sprite), key, false);
                component.HiddenLayers.Add(key);
            }
        }

        // Show any layers that have not been explicitly mentioned
        foreach (var key in toShow)
        {
            _sprite.LayerSetVisible((uid, sprite), key, true);
        }
        // End DeltaV Additions - Hideable Layers

        // So they don't draw over mobs anymore
        if (data == MobState.Dead)
        {
            if (sprite.DrawDepth > (int)DrawDepth.DeadMobs)
            {
                component.OriginalDrawDepth = sprite.DrawDepth;
                _sprite.SetDrawDepth((uid, sprite), (int)DrawDepth.DeadMobs);
            }
        }
        else if (component.OriginalDrawDepth != null)
        {
            _sprite.SetDrawDepth((uid, sprite), component.OriginalDrawDepth.Value);
            component.OriginalDrawDepth = null;
        }
    }
}
