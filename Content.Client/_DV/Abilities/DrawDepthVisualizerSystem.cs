using Content.Shared.DrawDepth;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Abilities;

/// <summary>
/// Changes a sprite's draw depth when some appearance data becomes true.
/// </summary>
public sealed class DrawDepthVisualizerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DrawDepthVisualizerComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<DrawDepthVisualizerComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite is not {} sprite || !args.AppearanceData.TryGetValue(ent.Comp.Key, out var value))
            return;

        if (value is true)
        {
            if (ent.Comp.OriginalDrawDepth != null)
                return;

            ent.Comp.OriginalDrawDepth = sprite.DrawDepth;
            sprite.DrawDepth = (int) ent.Comp.Depth;
        }
        else
        {
            if (ent.Comp.OriginalDrawDepth is not {} original)
                return;

            sprite.DrawDepth = original;
            ent.Comp.OriginalDrawDepth = null;
        }
    }
}
