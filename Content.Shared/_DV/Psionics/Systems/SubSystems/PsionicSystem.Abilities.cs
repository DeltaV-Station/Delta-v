using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Popups;

namespace Content.Shared._DV.Psionics.Systems;

public sealed partial class PsionicSystem
{
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    private void InitializeAbilities()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicComponent, PsionicPowerUsedEvent>(OnAbilityUsed);

    }

    private void OnAbilityUsed(Entity<PsionicComponent> psionic, ref PsionicPowerUsedEvent args)
    {
        var coords = Transform(psionic).Coordinates;

        foreach (var detector in _lookupSystem.GetEntitiesInRange<MetapsionicPowerComponent>(coords, 10f))
        {
            if (detector.Owner == psionic.Owner
                || TryComp<PsionicallyInsulatedComponent>(detector, out var insulated)
                && !insulated.AllowsPsionicUsage)
                continue;

            var ev = new PsionicPowerDetectedEvent(psionic, args.Power);
            RaiseLocalEvent(detector, ref ev);

            _popupSystem.PopupEntity(Loc.GetString("metapsionic-pulse-power-detected", ("power", args.Power)), detector, detector, PopupType.LargeCaution);
        }

        args.Handled = true;
    }
}
