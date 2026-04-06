using System.Linq;
using Content.Shared._DV.Psionics.Components;
using Content.Shared.EntityTable;
using Content.Shared.Popups;
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
        Dirty(potPsionic);
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

    public void AddRandomPsionicPower(Entity<PotentialPsionicComponent> psionic, bool midRound)
    {
        if (!_prototypeManager.Resolve(psionic.Comp.PsionicPowerTableId, out var powerTable))
            return;

        var random = Random.GetRandom(); // This is called in GetSpawns(). We simply call it once to avoid calling it multiple times.
        var attempts = 0;
        while (attempts < 20) // 20 attempts to get a unique psionic power.
        {
            var spawns = _entityTable.GetSpawns(powerTable, random);

            foreach (var entProtoId in spawns)
            {
                if (TryAddPsionicPower(psionic, midRound, entProtoId))
                    return;

                attempts++;
            }
        }

        Popup.PopupEntity(Loc.GetString("psionic-roll-failed"), psionic, psionic, PopupType.Medium);
    }

    private bool TryAddPsionicPower(Entity<PotentialPsionicComponent> psionic, bool midRound, EntProtoId entProtoId)
    {
        if (!_prototypeManager.Resolve(entProtoId, out var powerEntity))
            return false;
        // If the psionic already has that power, do not add it again.
        if (powerEntity.Components.Any(psionicComponent =>
                EntityManager.HasComponent(psionic, psionicComponent.Value.Component.GetType())))
            return false;
        // If they don't have it already, add it.
        EntityManager.AddComponents(psionic, powerEntity);

        if (!midRound)
            return true;
        // For alternative means of getting psionics that aren't via spawning in, cause them to suffer.
        _stuttering.DoStutter(psionic, TimeSpan.FromMinutes(1), false);
        _stun.TryKnockdown(psionic.Owner, TimeSpan.FromSeconds(3), false, drop: false);
        _jittering.DoJitter(psionic, TimeSpan.FromSeconds(3), false);

        return true;
    }

    public bool GrantPsionicRoll(Entity<PotentialPsionicComponent?> potPsionic)
    {
        if (!Resolve(potPsionic, ref potPsionic.Comp) || !potPsionic.Comp.Rolled)
            return false;

        potPsionic.Comp.Rolled = false;
        Dirty(potPsionic);
        return true;
    }
}
