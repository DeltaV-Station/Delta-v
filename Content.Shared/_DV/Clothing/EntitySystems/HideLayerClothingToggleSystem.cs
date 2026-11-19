using Content.Shared._DV.Clothing.Components;
using Content.Shared._DV.Clothing.Events;
using Content.Shared.Inventory;
using Content.Shared.Verbs;
using JetBrains.Annotations;

namespace Content.Shared._DV.Clothing.EntitySystems;

public sealed class HideLayerClothingToggleSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HideLayerClothingToggleComponent, ClothingHideLayerAttemptEvent>(OnHideLayerAttempt);
        SubscribeLocalEvent<HideLayerClothingToggleComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerb);
    }

    private void OnHideLayerAttempt(Entity<HideLayerClothingToggleComponent> ent, ref ClothingHideLayerAttemptEvent args)
    {
        if (!ent.Comp.HideLayers)
            args.Cancel();
    }

    private void OnGetAlternativeVerb(Entity<HideLayerClothingToggleComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // disable the verb if the entity is being worn
        var verbDisabled = _inventory.TryGetContainingSlot(ent.Owner, out _);

        args.Verbs.Add(new AlternativeVerb
        {
            Text = ent.Comp.HideLayers ? "Show hair" : "Hide hair",
            Act = () => ToggleHideLayers(ent),
            Disabled = verbDisabled,
            Message = verbDisabled ? "Unequip to change" : null,
        });
    }

    [PublicAPI]
    public void SetHideLayers(Entity<HideLayerClothingToggleComponent> ent, bool state)
    {
        ent.Comp.HideLayers = state;
        Dirty(ent);
    }

    private void ToggleHideLayers(Entity<HideLayerClothingToggleComponent> ent)
    {
        SetHideLayers(ent, !ent.Comp.HideLayers);
    }
}
