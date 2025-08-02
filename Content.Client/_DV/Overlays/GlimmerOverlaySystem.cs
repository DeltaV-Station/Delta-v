using Content.Shared.Psionics.Glimmer;
using Robust.Client.Graphics;

namespace Content.Client._DV.Overlays;

public sealed partial class GlimmerOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly GlimmerSystem _glimmer = default!;

    private GlimmerOverlay _overlay = default!;

    public override void Initialize()
    {
        Log.Debug("dawg i fucking hate this system lowkey");
        base.Initialize();

        _overlayMan.AddOverlay(_overlay = new());
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        Log.Debug("hell yeah glimmer is like " + _glimmer.Glimmer);
        Log.Debug("erm wait our glimmer tier is like " + _glimmer.GetGlimmerTier());

        if(_glimmer.Glimmer > 750)
        {
            if (!_overlayMan.HasOverlay<GlimmerOverlay>())
            {
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
