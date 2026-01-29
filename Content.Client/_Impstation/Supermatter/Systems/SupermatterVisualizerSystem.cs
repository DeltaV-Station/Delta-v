using Content.Client._Impstation.Supermatter.Components;
using Content.Shared._Impstation.Supermatter.Components;
using Robust.Client.GameObjects;

namespace Content.Client._Impstation.Supermatter.Systems;

public sealed class SupermatterVisualizerSystem : VisualizerSystem<SupermatterVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, SupermatterVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;
        
        Entity<SpriteComponent?> ent = (uid, args.Sprite);
        var crystalLayer = SpriteSystem.LayerMapGet(ent, SupermatterVisuals.Crystal);
        var psyLayer = SpriteSystem.LayerMapGet(ent, SupermatterVisuals.Psy);
        
        if (AppearanceSystem.TryGetData(uid, SupermatterVisuals.Crystal, out SupermatterCrystalState crystalState, args.Component) &&
            component.CrystalVisuals.TryGetValue(crystalState, out var crystalData))
        {
            SpriteSystem.LayerSetRsiState(ent, crystalLayer, crystalData.State);
        }

        if (AppearanceSystem.TryGetData(uid, SupermatterVisuals.Psy, out float psyState, args.Component))
        {
            var color = new Color(1f, 1f, 1f, psyState);
            SpriteSystem.LayerSetColor(ent, psyLayer, color);
        }
        else
        {
            SpriteSystem.LayerSetColor(ent, psyLayer, Color.Transparent);
        }
    }
}
