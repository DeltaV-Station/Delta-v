using Content.Server._DV.StationEvents.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Shared._Goobstation.StationEvents.Metric;
using Content.Shared.GameTicking.Components;
using Content.Shared.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._DV;

public sealed class GameDirectorEventTaggingTest
{
    [Test]
    public async Task AllEventsTaggedTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var componentFactory = server.ResolveDependency<IComponentFactory>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var emptyChaos = new ChaosMetrics();

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                foreach (var gameRule in protoMan.EnumeratePrototypes<EntityPrototype>())
                {
                    if (gameRule.Abstract)
                        continue;

                    if (!gameRule.HasComponent<GameRuleComponent>())
                        continue;

                    if (!gameRule.HasComponent<StationEventComponent>())
                        continue;

                    if (gameRule.HasComponent<DynamicRulesetComponent>() || gameRule.HasComponent<GameDirectorIgnoreComponent>())
                        continue;

                    gameRule.TryGetComponent<StationEventComponent>(out var stationEvent, componentFactory);
                    Assert.That(stationEvent, Is.Not.Null, $"Station event {gameRule.ID} should have the StationEventComponent");

                    Assert.That(stationEvent.Chaos, Is.Not.EqualTo(emptyChaos), $"Station event {gameRule.ID} should have a non-empty chaos value");
                }
            });
        });
    }
}
