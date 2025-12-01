using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Popups;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public sealed partial class TestPowerSystem : BasePsionicPowerSystem<TestPowerComponent, TestPowerActionEvent>
{
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;

    protected override void OnPowerUsed(Entity<TestPowerComponent> psionic, ref TestPowerActionEvent args)
    {
        PopupSystem.PopupClient(Loc.GetString("psionic-power-metapsionic-failure"), args.Performer, args.Performer, PopupType.Large);
        LogPowerUsed(args.Performer, psionic.Comp.PowerName, psionic.Comp.MinGlimmerChanged, psionic.Comp.MaxGlimmerChanged);

        args.Handled = true;
    }
}
