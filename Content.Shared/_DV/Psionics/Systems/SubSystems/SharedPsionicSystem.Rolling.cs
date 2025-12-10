using System.Linq;
using Content.Shared._DV.Psionics.Components;
using Content.Shared.EntityTable;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._DV.Psionics.Systems;

public abstract partial class SharedPsionicSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;

    public bool TryRollPsionic(Entity<PotentialPsionicComponent> potPsionic, float multiplier = 1.0f)
    {
        if (potPsionic.Comp.Rolled)
            return false;

        potPsionic.Comp.Rolled = true;

        if (!RollChance(potPsionic, multiplier))
        {
            Popup.PopupEntity(Loc.GetString("psionic-roll-failed"), potPsionic, potPsionic, PopupType.Medium);
            return false;
        }

        AddRandomPsionicPower(potPsionic, true);
        return true;
    }

    protected bool RollChance(Entity<PotentialPsionicComponent> potPsionic, float multiplier = 1.0f)
    {
        var chance = potPsionic.Comp.BaseChance;
        // Jobs like Command and Chaplains get a bonus on their roll.
        chance += potPsionic.Comp.JobBonusChance;
        // Species like Kitsunes get a bonus on their roll.
        chance += potPsionic.Comp.SpeciesBonusChance;

        // Rolling with chemicals can have multipliers.
        chance *= multiplier;

        chance = Math.Clamp(chance, 0, 1);
        return Random.Prob(chance);
    }

    public void AddRandomPsionicPower(Entity<PotentialPsionicComponent> psionic, bool midRound, int attempts = 0)
    {
        // This is important to check for beings that become potentially psionic through other ways than spawning in.
        // Example would be admeme or cognizine.
        if (psionic.Comp.AvailablePsionics == null)
        {
            if (!_prototypeManager.Resolve(psionic.Comp.PsionicPowerTableId, out var powerTable))
                return;

            psionic.Comp.AvailablePsionics = powerTable.Table;
        }

        var spawns = _entityTable.GetSpawns(psionic.Comp.AvailablePsionics);

        foreach (var entProtoId in spawns)
        {
            if (!_prototypeManager.Resolve(entProtoId, out var psionicComponents))
                continue;
            // If the psionic already has that power, do not add it again and retry again.
            if (psionicComponents.Components.Any(psionicComponent =>
                    EntityManager.HasComponent(psionic, psionicComponent.Value.Component.GetType())))
                continue;
            // If they don't have it already, add it and break out.
            EntityManager.AddComponents(psionic, psionicComponents);

            if (!midRound)
                return;
            // For alternative means of getting psionics that aren't via spawning in, cause them to suffer.
            _stuttering.DoStutter(psionic, TimeSpan.FromMinutes(1), false);
            _stun.TryKnockdown(psionic.Owner, TimeSpan.FromSeconds(3), false, drop: false);
            _jittering.DoJitter(psionic, TimeSpan.FromSeconds(5), false);

            return;
        }

        attempts++;
        if (attempts < 20) // 20 attempts to get a unique psionic power.
            AddRandomPsionicPower(psionic, midRound, attempts);
        else
        {
            Popup.PopupEntity(Loc.GetString("psionic-roll-failed"), psionic, psionic, PopupType.Medium);
        }
    }
}
