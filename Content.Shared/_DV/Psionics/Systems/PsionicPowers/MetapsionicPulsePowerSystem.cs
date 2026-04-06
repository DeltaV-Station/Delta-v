using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Popups;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This system allows a psionic user to spot other psionic entities via a pulse.
/// </summary>
public sealed class MetapsionicPulsePowerSystem : BasePsionicPowerSystem<MetapsionicPulsePowerComponent,  MetapsionicPulsePowerActionEvent>
{
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;

    protected override void OnPowerInit(Entity<MetapsionicPulsePowerComponent> power, ref MapInitEvent args)
    {
        base.OnPowerInit(power, ref args);

        EnsureComp<PsionicPowerDetectorComponent>(power);
    }

    protected override void OnPowerUsed(Entity<MetapsionicPulsePowerComponent> psionic, ref MetapsionicPulsePowerActionEvent args)
    {
        foreach (var target in _lookupSystem.GetEntitiesInRange<PsionicComponent>(args.Target, psionic.Comp.Range))
        {
            if (target.Owner == args.Performer
                || Transform(target).ParentUid == args.Performer)
                continue;

            if (!Psionic.CanBeTargeted(target)) // Cannot detect shielded psionics.
                continue;

            Popup.PopupClient(Loc.GetString("psionic-power-metapsionic-success"), args.Performer, args.Performer, PopupType.LargeCaution);
            AfterPowerUsed(psionic, args.Performer);

            args.Handled = true;
            return;
        }
        Popup.PopupClient(Loc.GetString("psionic-power-metapsionic-failure"), args.Performer, args.Performer, PopupType.Large);
        AfterPowerUsed(psionic, args.Performer);

        args.Handled = true;
    }

    protected override void OnMindBroken(Entity<MetapsionicPulsePowerComponent> psionic, ref PsionicMindBrokenEvent args)
    {
        base.OnMindBroken(psionic, ref args);
        // If the mindbreak was successful, remove the detector component too.
        if (!psionic.Comp.Deleted)
            return;

        RemComp<PsionicPowerDetectorComponent>(psionic);
    }
}
