using Content.Shared._DV.Silicon.IPC;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Silicon.IPC;

public sealed class SnoutHelmetSystem : VisualizerSystem<SnoutHelmetComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SnoutHelmetComponent, ComponentStartup>(OnComponentStartup);
    }

    public void OnComponentStartup(EntityUid uid, SnoutHelmetComponent component, ComponentStartup args)
    {
        if (TryComp(uid, out HumanoidAppearanceComponent? humanoidAppearanceComponent) &&
            humanoidAppearanceComponent.ClientOldMarkings.Markings.TryGetValue(MarkingCategories.Snout, out var data) &&
            data.Count > 0)
        {
            component.EnableAlternateHelmet = true;
        }
    }
}
