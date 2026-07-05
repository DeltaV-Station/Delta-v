using Content.Shared._DV.Kitchen.Components;
using Content.Shared.Chemistry.Components;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Kitchen;

public sealed class DeepFryerVisualizerSystem : VisualizerSystem<DeepFryerComponent>
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    protected override void OnAppearanceChange(EntityUid uid, DeepFryerComponent component, ref AppearanceChangeEvent args)
    {
        if (!_appearance.TryGetData(uid, DeepFryerVisuals.Bubbling, out bool isBubbling, args.Component) ||
            !TryComp<SolutionContainerVisualsComponent>(uid, out var scvComponent))
        {
            return;
        }

        scvComponent.FillBaseName = isBubbling ? "on-" : "off-";
    }
}
