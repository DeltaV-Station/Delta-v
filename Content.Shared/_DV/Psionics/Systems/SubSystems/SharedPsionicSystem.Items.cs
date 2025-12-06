using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Shared._DV.Psionics.Systems;

public abstract partial class SharedPsionicSystem
{
    private void InitializeItems()
    {
        SubscribeLocalEvent<PsionicallyInsulativeComponent, GotEquippedEvent>(OnInsulativeGearEquipped);
        SubscribeLocalEvent<PsionicallyInsulativeComponent, GotUnequippedEvent>(OnInsulativeGearUnequipped);

        SubscribeLocalEvent<PsionicallyInsulativeComponent, InventoryRelayedEvent<PsionicPowerUseAttemptEvent>>(OnPowerUseAttempt);
        SubscribeLocalEvent<PsionicallyInsulativeComponent, InventoryRelayedEvent<CheckPsionicInsulativeGearEvent>>(OnPsionicGearChecked);
        SubscribeLocalEvent<PsionicallyInsulativeComponent, InventoryRelayedEvent<TargetedByPsionicPowerEvent>>(OnTargetedByPsionicPower);
    }

    private void OnInsulativeGearEquipped(Entity<PsionicallyInsulativeComponent> gear, ref GotEquippedEvent args)
    {
        RefreshPsionicAbilities(args.Equipee);
    }

    private void OnInsulativeGearUnequipped(Entity<PsionicallyInsulativeComponent> gear, ref GotUnequippedEvent args)
    {
        RefreshPsionicAbilities(args.Equipee);
    }

    private void RefreshPsionicAbilities(EntityUid user)
    {
        if (!TryComp<PsionicComponent>(user, out var psionic))
            return;

        var ev = new CheckPsionicInsulativeGearEvent();
        RaiseLocalEvent(user, ref ev);
    }

    private void OnPsionicGearChecked(Entity<PsionicallyInsulativeComponent> gear, ref InventoryRelayedEvent<CheckPsionicInsulativeGearEvent> args)
    {
        args.Args.GearPresent = true;
        // If one gear blocks psionic usage, psionics cannot be used.
        args.Args.AllowsPsionicUsage &= gear.Comp.AllowsPsionicUsage;
        // If one gear shields from psionics, they're shielded.
        args.Args.ShieldsFromPsionics |= gear.Comp.ShieldsFromPsionics;

    }

    private void OnPowerUseAttempt(Entity<PsionicallyInsulativeComponent> gear, ref InventoryRelayedEvent<PsionicPowerUseAttemptEvent> args)
    {
        // If one gear blocks psionic usage, psionics cannot be used.
        args.Args.CanUsePower &= gear.Comp.AllowsPsionicUsage;
    }

    private void OnTargetedByPsionicPower(Entity<PsionicallyInsulativeComponent> gear, ref InventoryRelayedEvent<TargetedByPsionicPowerEvent> args)
    {
        // If one gear shields from psionics, they're shielded.
        args.Args.IsShielded |= gear.Comp.ShieldsFromPsionics;
    }
}
