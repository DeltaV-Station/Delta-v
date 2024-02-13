using Content.Shared.Abilities;
using Content.Shared.DeltaV.Abilities;

namespace Content.Client.DeltaV.Overlays;

public sealed partial class DefaultVisionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DefaultVisionComponent, ComponentInit>(OnDefaultVisionInit);
    }

    private void OnDefaultVisionInit(EntityUid uid, DefaultVisionComponent component, ComponentInit args)
    {
        RemComp<UltraVisionComponent>(uid);
    }
}
