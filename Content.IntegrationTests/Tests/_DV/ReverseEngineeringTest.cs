using System.Collections.Generic;
using Content.Shared.Lathe;
using Content.Shared.ReverseEngineering;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._DV;

[TestFixture]
public sealed class ReverseEngineeringTest
{
    [Test]
    public async Task AllReverseEngineeredPrintableTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var protoManager = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            var lathes = new List<LatheComponent>();
            var reverseEngineered = new HashSet<string>();
            foreach (var proto in protoManager.EnumeratePrototypes<EntityPrototype>())
            {
                if (proto.Abstract)
                    continue;

                if (pair.IsTestPrototype(proto))
                    continue;

                if (proto.TryGetComponent<LatheComponent>(out var lathe))
                    lathes.Add(lathe);

                if (!proto.TryGetComponent<ReverseEngineeringComponent>(out var rev))
                    continue;

                foreach (var recipe in rev.Recipes)
                {
                    reverseEngineered.Add(recipe);
                }
            }

            var latheRecipes = new HashSet<string>();
            foreach (var lathe in lathes)
            {
                if (lathe.DynamicRecipes == null)
                    continue;

                foreach (var recipe in lathe.DynamicRecipes)
                {
                    latheRecipes.Add(recipe);
                }
            }

            Assert.Multiple(() =>
            {
                foreach (var recipe in reverseEngineered)
                {
                    Assert.That(latheRecipes, Does.Contain(recipe), $"Reverse engineered recipe \"{recipe}\" cannot be unlocked on any lathe.");
                }
            });
        });

        await pair.CleanReturnAsync();
    }
}
