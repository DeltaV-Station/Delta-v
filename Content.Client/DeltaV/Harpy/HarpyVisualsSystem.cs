using Content.Shared.DeltaV.Harpy;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;
using Content.Shared.Humanoid;

namespace Content.Client.DeltaV.Harpy;

public sealed class HarpyVisualsSystem : VisualizerSystem<HarpyVisualsComponent>
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;

    protected override void OnAppearanceChange(EntityUid uid, HarpyVisualsComponent component,
        ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid)) return;

        _appearance.TryGetData(uid, HardsuitWings.Worn, out bool worn);
        if (humanoid.MarkingSet.TryGetCategory(MarkingCategories.Tail, out var tailMarkings))
        {
            foreach (var markings in tailMarkings)
            {
                var markingId = markings.MarkingId;
                if (!_markingManager.TryGetMarking(markings, out var proto)) return;
                var sprites = proto.Sprites;
                foreach (var markingState in sprites)
                {
                    switch (markingState)
                    {
                        case SpriteSpecifier.Rsi rsi:
                            string taillayer = $"{markingId}-{rsi.RsiState}";
                            args.Sprite.LayerSetVisible(taillayer, !worn);
                            break;
                    }
                }
            }
        }

        if (humanoid.MarkingSet.TryGetCategory(MarkingCategories.Arms, out var armMarkings))
        {
            foreach (var markings in armMarkings)
            {
                var markingId = markings.MarkingId;
                if (!_markingManager.TryGetMarking(markings, out var proto)) return;
                var sprites = proto.Sprites;
                foreach (var markingState in sprites)
                {
                    switch (markingState)
                    {
                        case SpriteSpecifier.Rsi rsi:
                            string armlayer = $"{markingId}-{rsi.RsiState}";
                            args.Sprite.LayerSetVisible(armlayer, !worn);
                            break;
                    }
                }
            }
        }
    }
}
