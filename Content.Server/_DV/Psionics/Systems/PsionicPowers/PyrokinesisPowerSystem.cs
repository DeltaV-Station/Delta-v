using Content.Server.Atmos.EntitySystems;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Content.Shared.Atmos.Components;
using Content.Shared.Popups;

namespace Content.Server._DV.Psionics.Systems.PsionicPowers;

public sealed class PyrokinesisPowerSystem : SharedPyrokinesisPowerSystem
{
    [Dependency] private readonly FlammableSystem _flammableSystem = default!;

    protected override void OnPowerUsed(Entity<PyrokinesisPowerComponent> psionic, ref PyrokinesisPowerActionEvent args)
    {
        if (!Psionic.CanBeTargeted(args.Target, hasAggressor: args.Performer) || !TryComp<FlammableComponent>(args.Target, out var flammableComponent))
            return;

        flammableComponent.FireStacks += psionic.Comp.AddedFirestacks;
        _flammableSystem.Ignite(args.Target, args.Target);
        Popup.PopupEntity(Loc.GetString("pyrokinesis-power-used", ("target", args.Target)), args.Target, args.Performer, PopupType.LargeCaution);

        AfterPowerUsed(psionic, args.Performer);
    }
}
