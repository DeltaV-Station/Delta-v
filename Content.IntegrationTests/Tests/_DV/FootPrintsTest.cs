using Content.Server._DV.Footprints.Components;

namespace Content.IntegrationTests.Tests._DV;

public sealed class FootPrintsTest
{
    [Test]
    public async Task ValidatePrototypes()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var protos = pair.GetPrototypesWithComponent<FootPrintsComponent>();

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                foreach (var (proto, comp) in protos)
                {
                    Assert.That(comp.StepSize, Is.Positive, $"{proto.ID} holds invalid value for FootPrints stepSize.");
                    Assert.That(comp.DragSize, Is.Positive, $"{proto.ID} holds invalid value for FootPrints dragSize.");
                }
            });
        });

        await pair.CleanReturnAsync();
    }
}
