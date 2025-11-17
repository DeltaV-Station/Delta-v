using System.Linq;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Humanoid;

namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem
{
    private static readonly BodyPartType[] _asymmetric =
    [
        BodyPartType.Other,
        BodyPartType.Torso,
        BodyPartType.Tail,
        BodyPartType.Head,
    ];

    private static readonly BodyPartType[] _symmetric =
    [
        BodyPartType.Arm,
        BodyPartType.Hand,
        BodyPartType.Leg,
        BodyPartType.Foot,
    ];

    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;

    private void MapInitAppearance(Entity<BodyComponent> ent)
    {
        var symmetries = Enum.GetValues<BodyPartSymmetry>();

        foreach (var part in _asymmetric)
        {
            if (part.ToHumanoidLayers(BodyPartSymmetry.None) is not {} layer)
                continue;

            var layers = HumanoidVisualLayersExtension.Sublayers(layer);
            var visible = GetBodyChildrenOfType(ent, part).Any();

            _humanoidAppearance.SetLayersVisibility(ent.Owner, layers, visible: visible);
        }

        foreach (var part in _symmetric)
        {
            foreach (var side in symmetries)
            {
                if (part.ToHumanoidLayers(side) is not {} layer)
                    continue;

                var layers = HumanoidVisualLayersExtension.Sublayers(layer);
                var visible = GetBodyChildrenOfType(ent, part, symmetry: side).Any();

                _humanoidAppearance.SetLayersVisibility(ent.Owner, layers, visible: visible);
            }
        }
    }
}
