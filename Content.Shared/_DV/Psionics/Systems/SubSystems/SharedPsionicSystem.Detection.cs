using System.Linq;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Clothing;
using Content.Shared.Popups;

namespace Content.Shared._DV.Psionics.Systems;

public abstract partial class SharedPsionicSystem
{
    private void InitializeDetection()
    {
        SubscribeLocalEvent<PsionicComponent, PsionicPowerUsedEvent>(OnPowerUsed);

        SubscribeLocalEvent<PsionicPowerDetectorComponent, ClothingGotEquippedEvent>(OnGrantingClothingEquipped);
        SubscribeLocalEvent<PsionicPowerDetectorComponent, ClothingGotUnequippedEvent>(OnGrantingClothingUnequipped);
    }

    private void OnGrantingClothingEquipped(Entity<PsionicPowerDetectorComponent> detector, ref ClothingGotEquippedEvent args)
    {
        if (!HasComp<PotentialPsionicComponent>(args.Wearer)) // IPCs and non-player organics shouldn't be able to use abilities.
            return;

        detector.Comp.Wearer = args.Wearer;
        Dirty(detector);
    }

    private void OnGrantingClothingUnequipped(Entity<PsionicPowerDetectorComponent> detector, ref ClothingGotUnequippedEvent args)
    {
        detector.Comp.Wearer = null;
        Dirty(detector);
    }

    private void OnPowerUsed(Entity<PsionicComponent> psionic, ref PsionicPowerUsedEvent args)
    {
        var coords = Transform(args.User).Coordinates;

        foreach (var detector in _lookup.GetEntitiesInRange<PsionicPowerDetectorComponent>(coords, 10f).Select(detectorPower => detectorPower.Comp.Wearer ?? detectorPower.Owner))
        {
            if (detector == args.User)
                continue;

            var ev = new PsionicPowerUseAttemptEvent();
            RaiseLocalEvent(detector, ref ev);

            if (!ev.CanUsePower)
                continue;

            var detectEv = new PsionicPowerDetectedEvent(args.User, args.Power);
            RaiseLocalEvent(detector, ref detectEv);

            Popup.PopupEntity(Loc.GetString("psionic-power-metapsionic-power-detected", ("power", args.Power)), detector, detector, PopupType.LargeCaution);
        }

        args.Handled = true;
    }
}
