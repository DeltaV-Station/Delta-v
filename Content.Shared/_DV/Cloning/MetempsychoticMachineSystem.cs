using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._DV.Cloning;

public sealed class MetempsychoticMachineSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    /// <summary>
    /// Gets a string of an entity which to spawn, based on the Weighted Tables for the Metem Psychotic Table.
    /// </summary>
    /// <param name="ent">Entity holding a Metempsychotic Machine Component, usually cloning pod</param>
    /// <param name="karmaBonus">Positive Bonus added onto the chance of becoming humanoid again</param>
    /// <param name="oldSpecies">Old Species of the Cloned person</param>
    /// <param name="outNewSpecies">Out Variable of the new Species</param>
    /// <param name="karma">Negative value representing the times a person has died and been cloned. Reduces Chance</param>
    /// <returns></returns>
    public string GetSpawnEntity(Entity<MetempsychoticMachineComponent> ent,
        float karmaBonus,
        SpeciesPrototype oldSpecies,
        out SpeciesPrototype? outNewSpecies,
        int? karma = null)
    {
        var chance = ent.Comp.HumanoidBaseChance + karmaBonus; // Unless changed, karma bonus and humanoid base chance come together to 1.0

        if (karma != null)
            chance -= (ent.Comp.HumanoidBaseChance * (float)karma);

        // In case our chance is higher than 1, we simply clone them into their old species back without randomizing
        if (chance > 1 && _random.Prob(chance - 1))
        {
            outNewSpecies = oldSpecies;
            return oldSpecies.Prototype;
        }

        chance = Math.Clamp(chance, 0, 1);
        if (_random.Prob(chance) &&
            _prototypeManager.TryIndex<WeightedRandomPrototype>(
                MetempsychoticMachineComponent.MetempsychoticHumanoidPool,
                out var humanoidPool) &&
            _prototypeManager.TryIndex<SpeciesPrototype>(humanoidPool.Pick(), out var speciesPrototype))
        {
            outNewSpecies = speciesPrototype;
            return speciesPrototype.Prototype;
        }
        else if (_prototypeManager.TryIndex<WeightedRandomPrototype>(MetempsychoticMachineComponent.MetempsychoticNonHumanoidPool, out var nonHumanoidPool) && _prototypeManager.TryIndex<EntityPrototype>(nonHumanoidPool.Pick(), out var entityPrototype))
        {
            outNewSpecies = null;
            return entityPrototype.ID;
        }
        else
        {
            outNewSpecies = null;
            return "MobHuman";
        }
    }
}
