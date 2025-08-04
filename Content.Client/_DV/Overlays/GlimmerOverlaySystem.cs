using Content.Shared.Psionics.Glimmer;
using Robust.Client.Graphics;

namespace Content.Client._DV.Overlays;

public sealed partial class GlimmerOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private GlimmerOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlayMan.AddOverlay(_overlay = new());
        SubscribeNetworkEvent<GlimmerChangedEvent>(OnGlimmerChanged);
    }

    private void OnGlimmerChanged(GlimmerChangedEvent eventArgs)
    {
        if(eventArgs.Glimmer > 700)
        {
            _overlay.ActualGlimmerLevel = eventArgs.Glimmer;
            if (!_overlayMan.HasOverlay<GlimmerOverlay>())
            {
                _overlay.Reset();
                _overlayMan.AddOverlay(_overlay);
            }
        }
        else
        {
            if (_overlayMan.HasOverlay<GlimmerOverlay>())
            {
                _overlayMan.RemoveOverlay(_overlay);
            }
        }
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlayMan.RemoveOverlay<GlimmerOverlay>();
    }

}
