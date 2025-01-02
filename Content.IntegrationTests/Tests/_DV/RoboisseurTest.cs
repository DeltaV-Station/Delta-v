using System.Linq;
using Content.Server.Roboisseur.Roboisseur;
using Content.Shared.Item;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._DV;

[TestFixture]
[TestOf(typeof(RoboisseurSystem))]
public sealed class RoboisseurTest
{
    [Test]
    public async Task AllRoboisseurItemsExist()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        // Per RobustIntegrationTest.cs, wait until state is settled to access it.
        await server.WaitIdleAsync();

        var mapManager = server.ResolveDependency<IMapManager>();
        var prototypeManager = server.ResolveDependency<IPrototypeManager>();
        var entityManager = server.ResolveDependency<IEntityManager>();
        var entitySystemManager = server.ResolveDependency<IEntitySystemManager>();

        var roboisseurSystem = entitySystemManager.GetEntitySystem<RoboisseurSystem>();
        var roboisseurComponent = new RoboisseurComponent();

        var testMap = await pair.CreateTestMap();

        await server.WaitAssertion(() =>
        {
            var allProtos = roboisseurComponent.Tier2Protos.Concat(roboisseurComponent.Tier3Protos)
                .Concat(roboisseurComponent.RobossuierRewards);
            var enumerable = allProtos as string[] ?? allProtos.ToArray();
            var blacklistedProtos = roboisseurComponent.BlacklistedProtos;
            var coordinates = testMap.GridCoords;

            Assert.That(enumerable.Any(), "Roboisseur has no valid prototypes!");

            foreach (var proto in enumerable)
            {
                Assert.That(prototypeManager.TryIndex(proto, out var _),
                    $"Roboisseur has invalid prototype {proto}!");

                var spawned = entityManager.SpawnEntity(proto, coordinates);

                Assert.That(entityManager.HasComponent<ItemComponent>(spawned),
                    $"Roboisseur can request non-item  {proto}");
            }

            foreach (var proto in blacklistedProtos)
            {
                Assert.That(prototypeManager.TryIndex(proto, out var _),
                    $"Roboisseur has invalid prototype {proto} in blacklist!");
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
