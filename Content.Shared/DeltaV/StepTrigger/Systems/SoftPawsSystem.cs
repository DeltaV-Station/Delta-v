using Content.Shared.Tag;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Client.Clothing;
using Content.Shared.Clothing.Components;

namespace Content.Shared.StepTrigger.Systems;

// This partial deals with equipment, i.e., felinid shoe or not to shoe.
public sealed partial class SoftPawsSystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    private const string ShoeSlot = "shoes";

    private void OnEquip(EntityUid uid, InventorySlotsComponent component, GotEquippedEvent args)
    {
        var user = args.Equipee;
        // have to be wearing the mask to use it, duh.
        if (!_inventory.TryGetSlotEntity(user, ShoeSlot, out _, inventory) & _tagSystem.HasTag(args.Tripper, "SoftPaws")) // || shoeEntity != uid)  // maskEntity to shoeEntity
            return;

        var comp = EnsureComp<ShoesRequiredStepTriggerComponent>(user);
    //    comp.VoiceName = component.LastSetName;

      //  _actions.AddAction(user, ref component.ActionEntity, component.Action, uid);
    }

    private void OnUnequip(EntityUid uid, InventorySlotsComponent component, GotUnequippedEvent args)
    {
        RemComp<ShoesRequiredStepTriggerComponent>(args.Equipee);
    }

   // private void TrySetLastKnownName(EntityUid maskWearer, string lastName)
   // {
   //     if (!HasComp<VoiceMaskComponent>(maskWearer)
   //         || !_inventory.TryGetSlotEntity(maskWearer, MaskSlot, out var maskEntity)
   //         || !TryComp<VoiceMaskerComponent>(maskEntity, out var maskComp))
   //     {
   //         return;
   //     }

   //     maskComp.LastSetName = lastName;
   // }
}
