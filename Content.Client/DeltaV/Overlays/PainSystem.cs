using Content.Shared.DeltaV.Pain;
using Robust.Client.Graphics;
using Robust.Shared.Player;

namespace Content.Client.DeltaV.Overlays;

public sealed partial class PainSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;

    private PainOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PainComponent, ComponentInit>(OnPainInit);
        SubscribeLocalEvent<PainComponent, ComponentShutdown>(OnPainShutdown);
        SubscribeLocalEvent<PainComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PainComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPainInit(Entity<PainComponent> ent, ref ComponentInit args)
    {
        if (ent.Owner == _playerMan.LocalEntity && !ent.Comp.Suppressed)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPainShutdown(Entity<PainComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _playerMan.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(Entity<PainComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        if (!ent.Comp.Suppressed)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(Entity<PainComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Handle showing/hiding overlay based on suppression status
        if (_playerMan.LocalEntity is not { } player)
            return;

        if (!TryComp<PainComponent>(player, out var comp))
            return;

        if (comp.Suppressed && _overlayMan.HasOverlay<PainOverlay>())
            _overlayMan.RemoveOverlay(_overlay);
        else if (!comp.Suppressed && !_overlayMan.HasOverlay<PainOverlay>())
            _overlayMan.AddOverlay(_overlay);
    }
}
