using Content.Shared._DV.CCVars;
using Content.Shared.Psionics.Glimmer;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;

namespace Content.Client._DV.Overlays;

public sealed partial class GlimmerOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private GlimmerOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new GlimmerOverlay();
        SubscribeNetworkEvent<GlimmerChangedEvent>(OnGlimmerChanged);
        Subs.CVar(_cfg, DCCVars.DisableGlimmerShader, OnDisableGlimmerShaderChanged);
    }

    private void OnGlimmerChanged(GlimmerChangedEvent eventArgs)
    {
        if(_cfg.GetCVar(DCCVars.DisableGlimmerShader))
            return;

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

    private void OnDisableGlimmerShaderChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (_overlay.ActualGlimmerLevel > 700)
            _overlayMan.AddOverlay(_overlay);
    }

}
