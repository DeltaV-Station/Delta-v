using Content.Shared._DV.Silicon.IPC;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Silicon.IPC;

public sealed class SnoutHelmetSystem : VisualizerSystem<SnoutHelmetComponent>
{
    private const MarkingCategories MarkingToQuery = MarkingCategories.Snout;
    private const int MaximumMarkingCount = 0;

    protected override void OnAppearanceChange(EntityUid uid, SnoutHelmetComponent component, ref AppearanceChangeEvent args)
    {
        if (TryComp(uid, out HumanoidAppearanceComponent? humanoidAppearanceComponent) &&
            humanoidAppearanceComponent.ClientOldMarkings.Markings.TryGetValue(MarkingToQuery, out var markings) &&
            markings.Count > MaximumMarkingCount)
        {
            component.EnableAlternateHelmet = true;
        }
    }
}
