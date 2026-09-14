using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;

namespace Content.Client._DV.Screens;

public sealed class DVTextVisualsSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IResourceCache _resource = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;

    private DVTextRenderingOverlay _textRendering = default!;

    private Font _font = default!;

    public override void Initialize()
    {
        base.Initialize();

        _textRendering = new(_sprite, _animationPlayer);
        _overlay.AddOverlay(_textRendering);
        _font = new VectorFont(_resource.GetResource<FontResource>("/Fonts/_DV/TinyUnicode.ttf"), 12);

        SubscribeLocalEvent<DVTextVisualsComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<DVTextVisualsComponent, ComponentShutdown>(OnComponentShutdown);

        SubscribeLocalEvent<DVTextVisualsComponent, AnimationCompletedEvent>(OnAnimationComplete);
    }

    public override void Shutdown()
    {
        _overlay.RemoveOverlay(_textRendering);
    }

    private void OnComponentInit(Entity<DVTextVisualsComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Token = _textRendering.QueueRender(ent, _font);
    }

    private void OnComponentShutdown(Entity<DVTextVisualsComponent> ent, ref ComponentShutdown args)
    {
        foreach (var row in ent.Comp.Rows)
        {
            row.Texture?.Dispose();
        }
        ent.Comp.Token?.Cancel();
    }

    private void OnAnimationComplete(Entity<DVTextVisualsComponent> ent, ref AnimationCompletedEvent args)
    {
        if (args.Key != DVTextRenderingOverlay.MarqueeKey || !args.Finished || ent.Comp.Animation is not { } animation)
            return;

        _animationPlayer.Play(ent.Owner, animation, DVTextRenderingOverlay.MarqueeKey);
    }

    public void SetText(Entity<DVTextVisualsComponent?> ent, params string[] rows)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        var count = Math.Min(rows.Length, ent.Comp.Rows.Count);
        for (var i = 0; i < count; i++)
        {
            ent.Comp.Rows[i].Text = rows[i];
        }

        ent.Comp.Token?.Cancel();
        ent.Comp.Token = _textRendering.QueueRender((ent, ent.Comp), _font);
    }
}
