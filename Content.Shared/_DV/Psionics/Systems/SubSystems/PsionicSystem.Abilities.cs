using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Popups;

namespace Content.Shared._DV.Psionics.Systems;

public sealed partial class PsionicSystem
{
    [Dependency] private readonly SharedActionsSystem _actionSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    private void InitializeAbilities()
    {
        base.Initialize();

        SubscribeLocalEvent<BasePsionicPower, ComponentInit>(OnPowerInit);
        SubscribeLocalEvent<BasePsionicPower, BaseActionEvent>(OnPowerActionUsed);

        SubscribeLocalEvent<PsionicComponent, PsionicPowerUsedEvent>(OnAbilityUsed);

    }

    private void OnPowerInit(Entity<BasePsionicPower> power, ref ComponentInit args)
    {
        _actionSystem.AddAction(power, ref power.Comp.ActionEntity, power.Comp.ActionProtoId );

        if (_actionSystem.GetAction(power.Comp.ActionEntity) is not { Comp.UseDelay: not null })
        {
            _actionSystem.StartUseDelay(power.Comp.ActionEntity);
        }

        if (TryComp<PsionicComponent>(power.Owner, out var psionic))
            psionic.PsionicPowersActionEntities.Add(power.Comp.ActionEntity);
    }

    private void OnPowerActionUsed(Entity<BasePsionicPower> psionic, ref BaseActionEvent args)
    {
        var ev = new PsionicPowerAttemptEvent();
        RaiseLocalEvent(psionic.Owner, ref ev);

        if (!ev.CanUsePower)
        {
            _popupSystem.PopupClient(Loc.GetString("psionic-cannot-use-psionics"), psionic);
            return;
        }
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
