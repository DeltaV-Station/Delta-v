using Content.Server.Roboisseur.Roboisseur;
using Content.Shared.Item;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using System.Collections.Generic;

namespace Content.IntegrationTests.Tests._DV;

[TestFixture]
[TestOf(typeof(RoboisseurSystem))]
public sealed class RoboisseurTest
{
    [Test]
    public async Task AllRoboisseurRewardsAreItems()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        // Per RobustIntegrationTest.cs, wait until state is settled to access it.
        await server.WaitIdleAsync();

        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        // TODO: spawn the actual prototype and get the component from it
        var comp = new RoboisseurComponent();

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                Check(comp.Tier2Protos, protoMan, factory);
                Check(comp.Tier3Protos, protoMan, factory);
                Check(comp.RobossuierRewards, protoMan, factory);
            });
        });

        await pair.CleanReturnAsync();
    }

    private void Check(List<EntProtoId> protos, IPrototypeManager protoMan, IComponentFactory factory)
    {
        foreach (var id in protos)
        {
            var proto = protoMan.Index(id);
            var isItem = proto.TryGetComponent<ItemComponent>(out _, factory);
            Assert.That(isItem, $"Roboisseur can request non-item {id}");
        }
    }
}
