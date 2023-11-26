using Content.Shared.Nyanotrasen.Lamiae;
using Content.Shared.Humanoid;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Client.GameObjects;
namespace Content.Client.Nyanotrasen.Lamiae;

public sealed class LamiaVisualizerSystem : SharedLamiaSystem
{
    public void UpdateAppearance(EntityUid uid, LamiaSegmentComponent? lamia = null)
    {
        if (!Resolve(uid, ref lamia))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (TryComp<HumanoidAppearanceComponent>(lamia, out var humanoid))
        {
            foreach (var marking in humanoid.MarkingSet.GetForwardEnumerator())
            {
                if (marking.MarkingId != "LamiaBottom")
                    continue;

                var color = marking.MarkingColors[0];
                sprite.LayerSetColor("enum.LamiaSegmentVisualLayers.Base", color);
            }
        }
    }
}

public enum LamiaVisualLayers
{
    Base,
    Armor,
}
