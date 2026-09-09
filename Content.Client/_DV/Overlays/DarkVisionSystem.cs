using Content.Shared._DV.Overlays.Components;
using Robust.Client.Graphics;
using Robust.Shared.Player;

namespace Content.Client._DV.Overlays;

public sealed class DarkVisionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;

    private DarkVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DarkVisionComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<DarkVisionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<DarkVisionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<DarkVisionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnInit(Entity<DarkVisionComponent> ent, ref ComponentInit args)
    {
        if (ent.Owner == _playerMan.LocalEntity)
            EnableOverlay(ent.Comp);
    }

    private void OnShutdown(Entity<DarkVisionComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _playerMan.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(Entity<DarkVisionComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        EnableOverlay(ent.Comp);
    }

    private void OnPlayerDetached(Entity<DarkVisionComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void EnableOverlay(DarkVisionComponent comp)
    {
        _overlay.LightFloor = comp.LightFloor;
        _overlay.LightGain = comp.LightGain;
        _overlay.LightExp = comp.LightExp;
        if (!_overlayMan.HasOverlay<DarkVisionOverlay>())
            _overlayMan.AddOverlay(_overlay);
    }
}
