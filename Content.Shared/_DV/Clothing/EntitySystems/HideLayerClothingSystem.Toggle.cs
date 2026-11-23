using Content.Shared.Clothing.Components;
using Content.Shared.Verbs;
using JetBrains.Annotations;

namespace Content.Shared.Clothing.EntitySystems;

public sealed partial class HideLayerClothingSystem
{
    private void InitializeToggle()
    {
        SubscribeLocalEvent<HideLayerClothingComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerb);
    }

    [PublicAPI]
    public void SetHideOverride(Entity<HideLayerClothingComponent> ent, bool state)
    {
        ent.Comp.BlockLayerHide = state;

        // try to get the wearer of the clothing
        if (TryComp(ent, out ClothingComponent? clothing)
            && clothing.InSlotFlag is { } slotFlag
            && clothing.Slots.HasFlag(slotFlag))
        {
            // update the layer state on the wearer
            var wearer = Transform(ent).ParentUid;
            SetLayerVisibility((ent, ent.Comp, clothing), wearer, !state);
        }

        Dirty(ent);
    }

    private void OnGetAlternativeVerb(Entity<HideLayerClothingComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // don't add the verb if we can't override
        if (!ent.Comp.AllowOverride)
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = ent.Comp.BlockLayerHide
                ? Loc.GetString("hidelayerclothing-component-toggle-off")
                : Loc.GetString("hidelayerclothing-component-toggle-on"),
            Act = () => SetHideOverride(ent, !ent.Comp.BlockLayerHide),
            Priority = -1, // so that alt click works as before (mostly the justice helmet's power cell eject)
        });
    }
}
