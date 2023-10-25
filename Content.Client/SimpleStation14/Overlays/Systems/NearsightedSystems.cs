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
        // Note: it would be cleaner to check if the glasses are being equipped
        // to the eyes rather than the pockets using `args.SlotFlags.HasFlag(SlotFlags.EYES)`,
        // but this field is not present on GotUnequippedEvent. This method is
        // used for both equip and unequip to make it consistent between checks.
        if (TryComp<NearsightedComponent>(args.Equipee, out var nearsighted) &&
            EnsureComp<TagComponent>(args.Equipment).Tags.Contains("GlassesNearsight") &&
            args.Slot == "eyes")
            UpdateShader(nearsighted, true);
    }

    private void OnUnEquip(GotUnequippedEvent args)
    {
        if (TryComp<NearsightedComponent>(args.Equipee, out var nearsighted) &&
            EnsureComp<TagComponent>(args.Equipment).Tags.Contains("GlassesNearsight") &&
            args.Slot == "eyes")
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
