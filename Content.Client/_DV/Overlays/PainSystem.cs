using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Robust.Client.Graphics;
using Robust.Shared.Player;

namespace Content.Client._DV.Overlays;

public sealed partial class PainSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;

    private PainOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NerveSystemComponent, ComponentInit>(OnPainInit);
        SubscribeLocalEvent<NerveSystemComponent, ComponentShutdown>(OnPainShutdown);
        SubscribeLocalEvent<NerveSystemComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<NerveSystemComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPainInit(Entity<NerveSystemComponent> ent, ref ComponentInit args)
    {
        if (ent.Owner == _playerMan.LocalEntity)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPainShutdown(Entity<NerveSystemComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _playerMan.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(Entity<NerveSystemComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(Entity<NerveSystemComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }
}
