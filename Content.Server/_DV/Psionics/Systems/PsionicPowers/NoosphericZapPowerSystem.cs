using Content.Server.Electrocution;
using Content.Server.Lightning;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;

namespace Content.Server._DV.Psionics.Systems.PsionicPowers;

public sealed class NoosphericZapPowerSystem : SharedNoosphericZapPowerSystem
{
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!;

    protected override void OnPowerUsed(Entity<NoosphericZapPowerComponent> psionic, ref NoosphericZapPowerActionEvent args)
    {
        if (!Psionic.CanBeTargeted(args.Target, hasAggressor: args.Performer))
            return;

        _lightning.ShootLightning(args.Performer, args.Target, psionic.Comp.LightningPrototpyeId);
        _electrocution.TryDoElectrocution(args.Target, args.Performer, psionic.Comp.ShockDamage, psionic.Comp.StunDuration, true);

        AfterPowerUsed(psionic, args.Performer);
    }
}
