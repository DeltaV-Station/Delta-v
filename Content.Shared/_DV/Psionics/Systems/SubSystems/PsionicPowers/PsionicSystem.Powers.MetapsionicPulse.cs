using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Popups;

namespace Content.Shared._DV.Psionics.Systems;

public sealed partial class PsionicSystem
{
    private void InitializeMetapsionicPulse()
    {
        base.Initialize();

        SubscribeLocalEvent<MetapsionicPulsePowerComponent, ComponentInit>(OnPowerInit);
        SubscribeLocalEvent<MetapsionicPulsePowerComponent, MetapsionicPulseActionEvent>(OnPowerUsed);
        SubscribeLocalEvent<MetapsionicPulsePowerComponent, PsionicMindBrokenEvent>(OnMindBroken);
    }

    private void OnPowerInit(Entity<MetapsionicPulsePowerComponent> psionic, ref ComponentInit args)
    {
        OnPowerInit((psionic.Owner, psionic.Comp));
    }

    private void OnPowerUsed(Entity<MetapsionicPulsePowerComponent> psionic, ref MetapsionicPulseActionEvent args)
    {
        if (!OnPowerActionUsed((psionic.Owner, psionic.Comp)))
            return;

        foreach (var target in _lookupSystem.GetEntitiesInRange<PsionicComponent>(args.Target, psionic.Comp.Range))
        {
            if (target.Owner == psionic.Owner
                || HasComp<ClothingGrantPsionicPowerComponent>(target) && Transform(target).ParentUid == psionic.Owner)
                continue;

            var ev = new TargetedByPsionicPowerEvent();
            RaiseLocalEvent(target, ref ev);

            if (ev.IsShielded) // Cannot detect shielded psionics.
                continue;

            _popupSystem.PopupClient(Loc.GetString("psionic-power-metapsionic-success"), psionic, psionic, PopupType.LargeCaution);
            args.Handled = true;
            return;
        }
        _popupSystem.PopupClient(Loc.GetString("psionic-power-metapsionic-failure"), psionic, psionic, PopupType.Large);
        LogPowerUsed(psionic, psionic.Comp.PowerName, psionic.Comp.MinGlimmerChanged, psionic.Comp.MaxGlimmerChanged);

        args.Handled = true;
    }

    private void OnMindBroken(Entity<MetapsionicPulsePowerComponent> psionic, ref PsionicMindBrokenEvent args)
    {
        OnMindBroken((psionic.Owner, psionic.Comp));
    }
}
