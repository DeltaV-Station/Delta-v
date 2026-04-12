using Content.Shared._DV.Silicon.IPC;
using Content.Shared.Humanoid;

namespace Content.Client._DV.Silicon.IPC;

public sealed class SnoutHelmetSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SnoutHelmetComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(EntityUid uid, SnoutHelmetComponent component, ComponentStartup args)
    {
        if (TryComp(uid, out HumanoidProfileComponent? humanoidAppearanceComponent) &&
            _appearanceSystem.TryGetData(uid, HumanoidVisualLayers.Snout, out var markings))
        {
            component.EnableAlternateHelmet = true;
        }
    }
}
