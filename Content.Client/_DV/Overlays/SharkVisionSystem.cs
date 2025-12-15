using Content.Client.Overlays;
using Content.Shared._DV.Overlays.Components;
using Content.Shared._Goobstation.Overlays;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;

namespace Content.Client._Goobstation.Overlays;

public sealed class SharkVisionSystem : EquipmentHudSystem<SharkVisionComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private SharkVisionOverlay _sharkOverlay = default!;
    private BaseSwitchableOverlay<SharkVisionComponent> _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SharkVisionComponent, SwitchableOverlayToggledEvent>(OnToggle);

        _sharkOverlay = new SharkVisionOverlay();
        _overlay = new BaseSwitchableOverlay<SharkVisionComponent>();
    }

    protected override void OnRefreshComponentHud(Entity<SharkVisionComponent> ent,
        ref RefreshEquipmentHudEvent<SharkVisionComponent> args)
    {
        if (!ent.Comp.IsEquipment)
            base.OnRefreshComponentHud(ent, ref args);
    }

    protected override void OnRefreshEquipmentHud(Entity<SharkVisionComponent> ent,
        ref InventoryRelayedEvent<RefreshEquipmentHudEvent<SharkVisionComponent>> args)
    {
        if (ent.Comp.IsEquipment)
            base.OnRefreshEquipmentHud(ent, ref args);
    }

    private void OnToggle(Entity<SharkVisionComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        RefreshOverlay();
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<SharkVisionComponent> args)
    {
        base.UpdateInternal(args);
        SharkVisionComponent? tvComp = null;
        var lightRadius = 0f;
        foreach (var comp in args.Components)
        {
            if (!comp.IsActive && (comp.PulseTime <= 0f || comp.PulseAccumulator >= comp.PulseTime))
                continue;

            if (tvComp == null)
                tvComp = comp;
            else if (!tvComp.DrawOverlay && comp.DrawOverlay)
                tvComp = comp;
            else if (tvComp.DrawOverlay == comp.DrawOverlay && tvComp.PulseTime > 0f && comp.PulseTime <= 0f)
                tvComp = comp;

            lightRadius = MathF.Max(lightRadius, 1);
        }

        UpdateSharkOverlay(tvComp, lightRadius);
        UpdateOverlay(tvComp);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        _sharkOverlay.ResetLight(false);
        UpdateOverlay(null);
        UpdateSharkOverlay(null, 0f);
    }

    private void UpdateSharkOverlay(SharkVisionComponent? comp, float lightRadius)
    {
        _sharkOverlay.LightRadius = lightRadius;
        _sharkOverlay.Comp = comp;

        switch (comp)
        {
            case not null when !_overlayMan.HasOverlay<SharkVisionOverlay>():
                _overlayMan.AddOverlay(_sharkOverlay);
                break;
            case null:
                _overlayMan.RemoveOverlay(_sharkOverlay);
                _sharkOverlay.ResetLight();
                break;
        }
    }

    private void UpdateOverlay(SharkVisionComponent? tvComp)
    {
        _overlay.Comp = tvComp;

        switch (tvComp)
        {
            case { DrawOverlay: true } when !_overlayMan.HasOverlay<BaseSwitchableOverlay<SharkVisionComponent>>():
                _overlayMan.AddOverlay(_overlay);
                break;
            case null or { DrawOverlay: false }:
                _overlayMan.RemoveOverlay(_overlay);
                break;
        }

        // Night vision overlay is prioritized
        _overlay.IsActive = !_overlayMan.HasOverlay<BaseSwitchableOverlay<NightVisionComponent>>();
    }
}
