using Content.Server._DV.Cloning;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._DV;

[TestFixture]
public sealed class MetempsychosisTest
{
    [Test]
    public async Task AllHumanoidPoolSpeciesExist()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        // Per RobustIntegrationTest.cs, wait until state is settled to access it.
        await server.WaitIdleAsync();

        var prototypeManager = server.ResolveDependency<IPrototypeManager>();

        var metemComponent = new MetempsychoticMachineComponent();

        await server.WaitAssertion(() =>
        {
            prototypeManager.TryIndex(metemComponent.MetempsychoticHumanoidPool,
                out var humanoidPool);
            prototypeManager.TryIndex(metemComponent.MetempsychoticNonHumanoidPool,
                out var nonHumanoidPool);

            Assert.Multiple(() =>
            {
                Assert.That(humanoidPool, Is.Not.Null, "MetempsychoticHumanoidPool is null!");
                Assert.That(nonHumanoidPool, Is.Not.Null, "MetempsychoticNonHumanoidPool is null!");
                Assert.That(humanoidPool.Weights,
                    Is.Not.Empty,
                    "MetempsychoticHumanoidPool has no valid prototypes!");
                Assert.That(nonHumanoidPool.Weights,
                    Is.Not.Empty,
                    "MetempsychoticNonHumanoidPool has no valid prototypes!");
            });

            foreach (var key in humanoidPool.Weights.Keys)
            {
                Assert.That(prototypeManager.TryIndex<SpeciesPrototype>(key, out _),
                    $"MetempsychoticHumanoidPool has invalid prototype {key}!");
            }

            foreach (var key in nonHumanoidPool.Weights.Keys)
            {
                Assert.That(prototypeManager.TryIndex<EntityPrototype>(key, out _),
                    $"MetempsychoticNonHumanoidPool has invalid prototype {key}!");
            }
        });
        await pair.CleanReturnAsync();
    }
}
