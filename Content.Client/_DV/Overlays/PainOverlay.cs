using System.Numerics;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared.FixedPoint;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Overlays;

public sealed partial class PainOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    private EntityQuery<NerveSystemComponent> _query;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    private readonly ShaderInstance _painShader;
    private readonly ProtoId<ShaderPrototype> _shaderProto = "ChromaticAberration";

    public PainOverlay()
    {
        IoCManager.InjectDependencies(this);

        _query = _entity.GetEntityQuery<NerveSystemComponent>();

        _painShader = _proto.Index(_shaderProto).Instance().Duplicate();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_query.TryComp(_player.LocalEntity, out var comp) || comp.Pain == FixedPoint2.Zero)
            return false;

        return base.BeforeDraw(in args);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture is null || _player.LocalEntity is not {} player)
            return;

        var pain = _query.CompOrNull(player)?.Pain ?? FixedPoint2.Zero;
        var distortion = 0.0003f * pain;

        _painShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _painShader.SetParameter("DISTORTION", distortion.Float());

        var worldHandle = args.WorldHandle;
        var viewport = args.WorldBounds;
        worldHandle.SetTransform(Matrix3x2.Identity);
        worldHandle.UseShader(_painShader);
        worldHandle.DrawRect(viewport, Color.White);
        worldHandle.UseShader(null);
    }
}
