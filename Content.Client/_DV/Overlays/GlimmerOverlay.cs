using System.Diagnostics;
using System.Numerics;
using Content.Shared._DV.Pain;
using Content.Shared.Psionics;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Serilog;

namespace Content.Client._DV.Overlays;

public sealed partial class GlimmerOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    private readonly ShaderInstance _glimmerShader;
    private readonly ProtoId<ShaderPrototype> _shaderProto = "HighGlimmer";

    public GlimmerOverlay()
    {
        IoCManager.InjectDependencies(this);
        _glimmerShader = _prototype.Index(_shaderProto).Instance().Duplicate();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is not { Valid: true } player)
        {
            return false;
        }

        return base.BeforeDraw(in args);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        //_glimmerShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        var worldHandle = args.WorldHandle;
        var viewport = args.WorldBounds;
        worldHandle.UseShader(_glimmerShader);
        worldHandle.DrawRect(viewport, Color.White);
        worldHandle.UseShader(null);
    }
}
