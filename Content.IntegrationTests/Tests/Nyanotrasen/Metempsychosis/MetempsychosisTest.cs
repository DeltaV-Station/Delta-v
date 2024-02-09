using System.Linq;
using Content.Server.Nyanotrasen.Cloning;
using Content.Shared.Random;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.DeltaV;

[TestFixture]
[TestOf(typeof(MetempsychoticMachineSystem))]
public sealed class MetempsychosisTest
{
    private readonly IPrototypeManager _prototypeManager = default!;

    [Test]
    public async Task AllHumanoidPoolSpeciesExist()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        // Per RobustIntegrationTest.cs, wait until state is settled to access it.
        await server.WaitIdleAsync();

        var mapManager = server.ResolveDependency<IMapManager>();
        var prototypeManager = server.ResolveDependency<IPrototypeManager>();
        var entityManager = server.ResolveDependency<IEntityManager>();
        var entitySystemManager = server.ResolveDependency<IEntitySystemManager>();

        var metemSystem = entitySystemManager.GetEntitySystem<MetempsychoticMachineSystem>();
        var metemComponent = new MetempsychoticMachineComponent();

        var testMap = await pair.CreateTestMap();

        await server.WaitAssertion(() =>
        {
            _prototypeManager.TryIndex<WeightedRandomPrototype>(metemComponent.MetempsychoticHumanoidPool, out var humanoidPool);
            _prototypeManager.TryIndex<WeightedRandomPrototype>(metemComponent.MetempsychoticNonHumanoidPool, out var nonHumanoidPool);

            var coordinates = testMap.GridCoords;

            Assert.That(humanoidPool.Weights.Any(), "MetempsychoticHumanoidPool has no valid prototypes!");
            Assert.That(nonHumanoidPool.Weights.Any(), "MetempsychoticNonHumanoidPool has no valid prototypes!");

            foreach (var (key, weight) in humanoidPool.Weights)
            {
                Assert.That(prototypeManager.TryIndex(key, out var _),
                    $"MetempsychoticHumanoidPool has invalid prototype {key}!");

                var spawned = entityManager.SpawnEntity(key, coordinates);
            }

            foreach (var (key, weight) in nonHumanoidPool.Weights)
            {
                Assert.That(prototypeManager.TryIndex(key, out var _),
                    $"MetempsychoticNonHumanoidPool has invalid prototype {key}!");

                var spawned = entityManager.SpawnEntity(key, coordinates);
            }

            // Because Server/Client pairs can be re-used between Tests, we
            // need to clean up anything that might affect other tests,
            // otherwise this pair cannot be considered clean, and the
            // CleanReturnAsync call would need to be removed.
            mapManager.DeleteMap(testMap.MapId);
        });
        await pair.CleanReturnAsync();
    }
}
