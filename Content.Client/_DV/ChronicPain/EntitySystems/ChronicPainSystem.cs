using Content.Client._DV.ChronicPain.Overlays;
using Content.Shared._DV.ChronicPain.Components;
using Content.Shared._DV.ChronicPain.EntitySystems;
using Robust.Client.Graphics;
using Robust.Shared.Player;

namespace Content.Client._DV.ChronicPain.EntitySystems;

public sealed class ChronicPainSystem : SharedChronicPainSystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    private ChronicPainOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new();
    }


    protected override void OnChronicPainInit(Entity<ChronicPainComponent> entity, ref ComponentInit args)
    {
        if (entity.Owner != _playerManager.LocalEntity)
            return;

        _overlayMan.AddOverlay(_overlay);
    }

    protected override void OnChronicPainShutdown(Entity<ChronicPainComponent> entity, ref ComponentShutdown args)
    {
        if (entity.Owner != _playerManager.LocalEntity)
            return;

        _overlayMan.RemoveOverlay(_overlay);
    }

    protected override void OnPlayerAttached(Entity<ChronicPainComponent> entity, ref LocalPlayerAttachedEvent args)
    {
        if (!IsChronicPainSuppressed((entity.Owner, entity.Comp)))
            _overlayMan.AddOverlay(_overlay);
    }

    protected override void OnPlayerDetached(Entity<ChronicPainComponent> entity, ref LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Handle showing/hiding overlay based on suppression status
        if (_playerManager.LocalEntity is not { } player)
            return;

        if (!TryComp<ChronicPainComponent>(player, out var comp))
            return;

        var isSuppressed = IsChronicPainSuppressed((player, comp));

        if (isSuppressed && _overlayMan.TryGetOverlay<ChronicPainOverlay>(out var overlay))
            _overlayMan.RemoveOverlay(overlay);

        if (isSuppressed) // If its suppressed and we don't have an overlay, just return
            return;

        if (!isSuppressed && !_overlayMan.HasOverlay<ChronicPainOverlay>())
            _overlayMan.AddOverlay(_overlay);
    }
}
