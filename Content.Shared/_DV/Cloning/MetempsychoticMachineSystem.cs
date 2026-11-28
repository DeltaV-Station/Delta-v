using System.Diagnostics.CodeAnalysis;
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
    /// <param name="outNewSpecies">Out Variable of the new Species</param>
    /// <param name="karma">Negative value representing the times a person has died and been cloned. Reduces Chance</param>
    /// <returns></returns>
    public string GetSpawnEntity(Entity<MetempsychoticMachineComponent> ent,
        out ProtoId<SpeciesPrototype>? outNewSpecies,
        int? karma = null)
    {
        var chance = ent.Comp.HumanoidBaseChance;
        chance -= karma != null ? (ent.Comp.NonHumanoidBaseChance * (float)karma) : 0.0f;
        chance = Math.Clamp(chance, 0, 1);

        if (_random.Prob(chance) && TryGetRandomHumanoidSpecies(out var speciesPrototype))
        {
            outNewSpecies = speciesPrototype;
            return speciesPrototype.Prototype;
        }

        if (TryGetRandomNonHumanoidEntity(out var entityPrototype))
        {
            outNewSpecies = null;
            return entityPrototype.ID;
        }

        // If all else fails, become human of sort
        outNewSpecies = null;
        return "MobHuman";
    }

    private bool TryGetRandomHumanoidSpecies([NotNullWhen(true)] out SpeciesPrototype? outSpecies)
    {
        if (_prototypeManager.TryIndex<WeightedRandomPrototype>(
                MetempsychoticMachineComponent.MetempsychoticHumanoidPool,
                out var humanoidPool) &&
            _prototypeManager.TryIndex<SpeciesPrototype>(humanoidPool.Pick(), out var speciesPrototype))
        {
            outSpecies = speciesPrototype;
            return true;
        }

        outSpecies = null;
        return false;
    }

    private bool TryGetRandomNonHumanoidEntity([NotNullWhen(true)] out EntityPrototype? outEntity)
    {
        if (_prototypeManager.TryIndex<WeightedRandomPrototype>(
                MetempsychoticMachineComponent.MetempsychoticNonHumanoidPool,
                out var nonHumanoidPool) &&
            _prototypeManager.TryIndex<EntityPrototype>(nonHumanoidPool.Pick(), out var entityPrototype))
        {
            outEntity = entityPrototype;
            return true;
        }

        outEntity = null;
        return false;
    }
}
