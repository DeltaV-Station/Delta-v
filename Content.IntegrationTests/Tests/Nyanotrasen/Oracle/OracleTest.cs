#nullable enable
using NUnit.Framework;
using System.Threading.Tasks;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Server.Research.Oracle;
using Content.Shared.Chemistry.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;


/// <summary>
/// The oracle's request pool is huge.
/// We need to test everything that the oracle could request can be turned in.
/// </summary>
namespace Content.IntegrationTests.Tests.Oracle
{
    [TestFixture]
    [TestOf(typeof(OracleSystem))]
    public sealed class OracleTest
    {
        [Test]
        public async Task AllOracleItemsCanBeTurnedIn()
        {
            await using var pairTracker = await PoolManager.GetServerClient();
            var server = pairTracker.Server;
            // Per RobustIntegrationTest.cs, wait until state is settled to access it.
            await server.WaitIdleAsync();

            var mapManager = server.ResolveDependency<IMapManager>();
            var prototypeManager = server.ResolveDependency<IPrototypeManager>();
            var entityManager = server.ResolveDependency<IEntityManager>();
            var entitySystemManager = server.ResolveDependency<IEntitySystemManager>();

            var oracleSystem = entitySystemManager.GetEntitySystem<OracleSystem>();
            var oracleComponent = new OracleComponent();

            var testMap = await pairTracker.CreateTestMap();

            await server.WaitAssertion(() =>
            {
                var allProtos = oracleSystem.GetAllProtos(oracleComponent);
                var coordinates = testMap.GridCoords;

                Assert.That((allProtos.Count > 0), "Oracle has no valid prototypes!");

                foreach (var proto in allProtos)
                {
                    var spawned = entityManager.SpawnEntity(proto, coordinates);

                    Assert.That(entityManager.HasComponent<ItemComponent>(spawned),
                        $"Oracle can request non-item {proto}");

                    Assert.That(!entityManager.HasComponent<SolutionTransferComponent>(spawned),
                        $"Oracle can request reagent container {proto} that will conflict with the fountain");

                    Assert.That(!entityManager.HasComponent<MobStateComponent>(spawned),
                        $"Oracle can request mob {proto} that could potentially have a player-set name.");
                }

                // Because Server/Client pairs can be re-used between Tests, we
                // need to clean up anything that might affect other tests,
                // otherwise this pair cannot be considered clean, and the
                // CleanReturnAsync call would need to be removed.
                mapManager.DeleteMap(testMap.MapId);
            });

            await pairTracker.CleanReturnAsync();
        }
    }
}
