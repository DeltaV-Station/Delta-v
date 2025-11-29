using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Popups;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public sealed partial class MetapsionicPulsePowerSystem : BasePsionicPowerSystem<MetapsionicPulsePowerComponent,  MetapsionicPulseActionEvent>
{
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;

    protected override void OnPowerUsed(Entity<MetapsionicPulsePowerComponent> psionic, ref MetapsionicPulseActionEvent args)
    {
        foreach (var target in _lookupSystem.GetEntitiesInRange<PsionicComponent>(args.Target, psionic.Comp.Range))
        {
            if (target.Owner == args.Performer
                || Transform(target).ParentUid == args.Performer)
                continue;

            var ev = new TargetedByPsionicPowerEvent();
            RaiseLocalEvent(target, ref ev);

            if (ev.IsShielded) // Cannot detect shielded psionics.
                continue;

            PopupSystem.PopupClient(Loc.GetString("psionic-power-metapsionic-success"), args.Performer, args.Performer, PopupType.LargeCaution);
            LogPowerUsed(args.Performer, psionic.Comp.PowerName, psionic.Comp.MinGlimmerChanged, psionic.Comp.MaxGlimmerChanged);

            args.Handled = true;
            return;
        }
        PopupSystem.PopupClient(Loc.GetString("psionic-power-metapsionic-failure"), args.Performer, args.Performer, PopupType.Large);
        LogPowerUsed(args.Performer, psionic.Comp.PowerName, psionic.Comp.MinGlimmerChanged, psionic.Comp.MaxGlimmerChanged);

        args.Handled = true;
    }
}
