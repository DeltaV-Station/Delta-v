using Content.Shared.Abilities;
using Robust.Client.Graphics;

namespace Content.Client.Nyanotrasen.Overlays;

public sealed partial class DogVisionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private DogVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DogVisionComponent, ComponentInit>(OnDogVisionInit);
        SubscribeLocalEvent<DogVisionComponent, ComponentShutdown>(OnDogVisionShutdown);

        _overlay = new();
    }

    private void OnDogVisionInit(EntityUid uid, DogVisionComponent component, ComponentInit args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnDogVisionShutdown(EntityUid uid, DogVisionComponent component, ComponentShutdown args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }
}
