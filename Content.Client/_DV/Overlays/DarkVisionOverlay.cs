using System.Numerics;
using Content.Client.Graphics;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Overlays;

/// <summary>
/// Makes darkness visible, and bright lights painfully visible
/// Tweakable. Algo is max((light*gain)^exp, lightFloor)
/// </summary>
public sealed class DarkVisionOverlay : Overlay
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowWorld;

    private readonly ProtoId<ShaderPrototype> _shaderProto = "DarkVision";

    public float LightFloor = 0.5f;
    public float LightGain = 2f;
    public float LightExp = 1f;

    private readonly ShaderInstance _copyShader;
    private readonly ShaderInstance _remapShader;
    private readonly OverlayResourceCache<CachedResources> _resources = new();

    public DarkVisionOverlay()
    {
        IoCManager.InjectDependencies(this);

        var proto = _prototype.Index<ShaderPrototype>(_shaderProto);
        _remapShader = proto.InstanceUnique();
        // With floor 0, gain 1, exp 1 the shader is an exact blend-mode-none copy.
        _copyShader = proto.InstanceUnique();
        _copyShader.SetParameter("lightFloor", 0f);
        _copyShader.SetParameter("lightGain", 1f);
        _copyShader.SetParameter("lightExp", 1f);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var viewport = args.Viewport;
        var worldHandle = args.WorldHandle;

        if (viewport.Eye == null)
            return;

        var lightTarget = viewport.LightRenderTarget;
        var res = _resources.GetForViewport(viewport, static _ => new CachedResources());

        if (res.ScratchTarget?.Size != lightTarget.Size)
        {
            res.ScratchTarget?.Dispose();
            res.ScratchTarget = _clyde.CreateLightRenderTarget(lightTarget.Size, "darkvision-scratch", depthStencil: false);
        }

        var bounds = args.WorldBounds;
        var lightScale = lightTarget.Size / (Vector2) viewport.Size;
        var scale = viewport.RenderScale / (Vector2.One / lightScale);
        var localMatrix = lightTarget.GetWorldToLocalMatrix(viewport.Eye, scale);

        // Copy the light buffer aside first: a texture can't be sampled while it is also the
        // render target being drawn into.
        worldHandle.RenderInRenderTarget(res.ScratchTarget, () =>
        {
            worldHandle.UseShader(_copyShader);
            worldHandle.SetTransform(localMatrix);
            worldHandle.DrawTextureRect(lightTarget.Texture, bounds);
            worldHandle.UseShader(null);
        }, Color.Black);

        // Then write it back through the remap.
        _remapShader.SetParameter("lightFloor", LightFloor);
        _remapShader.SetParameter("lightGain", LightGain);
        _remapShader.SetParameter("lightExp", LightExp);
        worldHandle.RenderInRenderTarget(lightTarget, () =>
        {
            worldHandle.UseShader(_remapShader);
            worldHandle.SetTransform(localMatrix);
            worldHandle.DrawTextureRect(res.ScratchTarget.Texture, bounds);
            worldHandle.UseShader(null);
        }, null);
    }

    protected override void DisposeBehavior()
    {
        _resources.Dispose();

        base.DisposeBehavior();
    }

    private sealed class CachedResources : IDisposable
    {
        public IRenderTexture? ScratchTarget;

        public void Dispose()
        {
            ScratchTarget?.Dispose();
        }
    }
}
