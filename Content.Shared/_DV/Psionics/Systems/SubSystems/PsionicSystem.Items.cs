using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Shared._DV.Psionics.Systems;

public sealed partial class PsionicSystem
{
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psiAbilities = default!;

    private void InitializeItems()
    {
        base.Initialize();
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

        foreach (var actionEntity in psionic.PsionicPowersActionEntities)
        {
            _actionSystem.SetEnabled(actionEntity, ev.AllowsPsionicUsage);
        }
    }

    private void OnPsionicGearChecked(Entity<PsionicallyInsulativeComponent> gear, ref InventoryRelayedEvent<CheckPsionicInsulativeGearEvent> args)
    {
        var evArgs = args.Args;

        evArgs.GearPresent = true;
        // If one gear blocks psionic usage, psionics cannot be used.
        evArgs.AllowsPsionicUsage = evArgs.AllowsPsionicUsage && gear.Comp.AllowsPsionicUsage;
        // If one gear shields from psionics, they're shielded.
        evArgs.ShieldsFromPsionics = evArgs.ShieldsFromPsionics || gear.Comp.ShieldsFromPsionics;
    }

    private void OnPowerUseAttempt(Entity<PsionicallyInsulativeComponent> gear, ref InventoryRelayedEvent<PsionicPowerUseAttemptEvent> args)
    {
        var evArgs = args.Args;
        // If one gear blocks psionic usage, psionics cannot be used.
        evArgs.CanUsePower = evArgs.CanUsePower && gear.Comp.AllowsPsionicUsage;
    }

    private void OnTargetedByPsionicPower(Entity<PsionicallyInsulativeComponent> gear, ref InventoryRelayedEvent<TargetedByPsionicPowerEvent> args)
    {
        var evArgs = args.Args;
        // If one gear shields from psionics, they're shielded.
        evArgs.IsShielded = evArgs.IsShielded || gear.Comp.ShieldsFromPsionics;
    }
}
