using Content.Client.SimpleStation14.Overlays.Shaders;
using Content.Shared.Inventory.Events;
using Content.Shared.SimpleStation14.Traits;
using Content.Shared.SimpleStation14.Traits.Components;
using Content.Shared.Tag;
using Robust.Client.Graphics;

namespace Content.Client.SimpleStation14.Overlays.Systems;

public sealed class NearsightedSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private NearsightedOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new NearsightedOverlay();

        SubscribeLocalEvent<NearsightedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<GotUnequippedEvent>(OnUnEquip);
    }


    private void OnStartup(EntityUid uid, NearsightedComponent component, ComponentStartup args)
    {
        UpdateShader(component, false);
    }

    private void OnEquip(GotEquippedEvent args)
    {
        if (TryComp<NearsightedComponent>(args.Equipee, out var nearsighted) &&
            EnsureComp<TagComponent>(args.Equipment).Tags.Contains("GlassesNearsight"))
            UpdateShader(nearsighted, true);
    }

    private void OnUnEquip(GotUnequippedEvent args)
    {
        if (TryComp<NearsightedComponent>(args.Equipee, out var nearsighted) &&
            EnsureComp<TagComponent>(args.Equipment).Tags.Contains("GlassesNearsight"))
            UpdateShader(nearsighted, false);
    }


    private void UpdateShader(NearsightedComponent component, bool booLean)
    {
        while (_overlayMan.HasOverlay<NearsightedOverlay>())
        {
            _overlayMan.RemoveOverlay(_overlay);
        }

        component.Active = booLean;
        _overlayMan.AddOverlay(_overlay);
    }
}
