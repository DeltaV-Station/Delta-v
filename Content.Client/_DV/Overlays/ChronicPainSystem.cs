using Content.Shared._DV.ChronicPain;
using Robust.Client.Graphics;
using Robust.Shared.Player;

namespace Content.Client._DV.Overlays;

public sealed partial class ChronicPainSystem : SharedChronicPainSystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;

    private ChronicPainOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChronicPainComponent, ComponentInit>(OnChronicPainInit);
        SubscribeLocalEvent<ChronicPainComponent, ComponentShutdown>(OnChronicPainShutdown);
        SubscribeLocalEvent<ChronicPainComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ChronicPainComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnChronicPainInit(Entity<ChronicPainComponent> ent, ref ComponentInit args)
    {
        if (ent.Owner == _playerMan.LocalEntity && !ent.Comp.Suppressed)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnChronicPainShutdown(Entity<ChronicPainComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _playerMan.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(Entity<ChronicPainComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        if (!ent.Comp.Suppressed)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(Entity<ChronicPainComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Handle showing/hiding overlay based on suppression status
        if (_playerMan.LocalEntity is not { } player)
            return;

        if (!TryComp<ChronicPainComponent>(player, out var comp))
            return;

        if (comp.Suppressed && _overlayMan.HasOverlay<ChronicPainOverlay>())
            _overlayMan.RemoveOverlay(_overlay);
        else if (!comp.Suppressed && !_overlayMan.HasOverlay<ChronicPainOverlay>())
            _overlayMan.AddOverlay(_overlay);
    }
}
