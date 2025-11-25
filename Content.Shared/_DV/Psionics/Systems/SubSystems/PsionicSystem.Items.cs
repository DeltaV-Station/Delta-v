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

        SubscribeLocalEvent<PsionicallyInsulativeComponent, InventoryRelayedEvent<CheckPsionicallyInsulativeGearEvent>>(OnPsionicGearChanged);
    }

    private void OnInsulativeGearEquipped(Entity<PsionicallyInsulativeComponent> gear, ref GotEquippedEvent args)
    {
        RefreshPsionicInsulation(args.Equipee);
    }

    private void OnInsulativeGearUnequipped(Entity<PsionicallyInsulativeComponent> gear, ref GotUnequippedEvent args)
    {
        RefreshPsionicInsulation(args.Equipee);
    }

    private void RefreshPsionicInsulation(EntityUid user)
    {
        var ev = new CheckPsionicallyInsulativeGearEvent();
        RaiseLocalEvent(user, ref ev);

        if (!ev.AllowsPsionicUsage && !ev.ShieldsFromPsionics)
        {
            RemComp<PsionicallyInsulatedComponent>(user);
            _psiAbilities.SetPsionicsThroughEligibility(user);
            return;
        }

        var insulationComp = EnsureComp<PsionicallyInsulatedComponent>(user);

        // insulationComp.AllowsPsionicUsage = ev.AllowsPsionicUsage ?? insulationComp.AllowsPsionicUsage;
        // insulationComp.ShieldsFromPsionics = ev.ShieldsFromPsionics ?? insulationComp.ShieldsFromPsionics;

        _psiAbilities.SetPsionicsThroughEligibility(user);
    }

    private void OnPsionicGearChanged(Entity<PsionicallyInsulativeComponent> gear, ref InventoryRelayedEvent<CheckPsionicallyInsulativeGearEvent> args)
    {
        var evArgs = args.Args;

        evArgs.AllowsPsionicUsage = false;
        // if (evArgs.AllowsPsionicUsage.HasValue)
        //     evArgs.AllowsPsionicUsage = evArgs.AllowsPsionicUsage.Value && gear.Comp.AllowsPsionicUsage;
        // else
        //     evArgs.AllowsPsionicUsage = gear.Comp.AllowsPsionicUsage;
        //
        // if (evArgs.ShieldsFromPsionics.HasValue)
        //     evArgs.ShieldsFromPsionics = evArgs.ShieldsFromPsionics.Value && gear.Comp.ShieldsFromPsionics;
        // else
        //     evArgs.ShieldsFromPsionics = gear.Comp.ShieldsFromPsionics;
    }
}
