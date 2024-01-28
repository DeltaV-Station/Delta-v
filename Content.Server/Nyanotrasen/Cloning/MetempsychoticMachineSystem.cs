using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Server.Nyanotrasen.Cloning
{
    public sealed class MetempsychoticMachineSystem : EntitySystem
    {
        [ValidatePrototypeId<WeightedRandomPrototype>]
        public const string MetempsychoticHumanoidPool = "MetempsychoticHumanoidPool";

        [ValidatePrototypeId<WeightedRandomPrototype>]
        public const string MetempsychoticNonHumanoidPool = "MetempsychoticNonhumanoidPool";

        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private ISawmill _sawmill = default!;

        public string GetSpawnEntity(EntityUid uid, float karmaBonus, SpeciesPrototype oldSpecies, out SpeciesPrototype? species, int? karma = null, MetempsychoticMachineComponent? component = null)
        {
            if (!Resolve(uid, ref component))
            {
                Logger.Error("Tried to get a spawn target from someone that was not a metempsychotic machine...");
                species = null;
                return "MobHuman";
            }

            var chance = component.HumanoidBaseChance + karmaBonus;

            if (karma != null)
                chance -= ((1 - component.HumanoidBaseChance) * (float) karma);

            if (chance > 1 && _random.Prob(chance - 1))
            {
                species = oldSpecies;
                return oldSpecies.Prototype;
            }
            else
                chance = 1;

            chance = Math.Clamp(chance, 0, 1);

            if (_random.Prob(chance) &&
                _prototypeManager.TryIndex<WeightedRandomPrototype>(MetempsychoticHumanoidPool, out var humanoidPool) &&
                _prototypeManager.TryIndex<SpeciesPrototype>(humanoidPool.Pick(), out var speciesPrototype))
            {
                species = speciesPrototype;
                return speciesPrototype.Prototype;
            }
            else
            {
                species = null;
                _sawmill.Error("Could not index species for metempsychotic machine...");
                return "MobHuman";
            }
        }
    }
}
