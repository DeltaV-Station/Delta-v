using Content.Shared._DV.Reputation;
using Content.Shared.Objectives.Components;
using Content.Shared.Prototypes;
using Content.Shared.Random;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using System.Collections.Generic;

namespace Content.IntegrationTests.Tests._DV;

/// <summary>
/// Checks that all ids in objective groups are valid
/// Objectives system just does nothing if you have a nonexistent prototype in an objective group.
/// </summary>
public sealed class ObjectivesTest
{
    [Test]
    public async Task AllReputationObjectivesValid()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        var pools = new HashSet<WeightedRandomPrototype>();
        foreach (var level in protoMan.EnumeratePrototypes<ReputationLevelPrototype>())
        {
            pools.Add(protoMan.Index(level.OfferingGroups));
        }

        await server.WaitPost(() =>
        {
            // 3 nested loops we gaming
            Assert.Multiple(() =>
            {
                foreach (var pool in pools)
                {
                    foreach (var id in pool.Weights.Keys)
                    {
                        Assert.That(protoMan.TryIndex<WeightedRandomPrototype>(id, out var proto),
                            $"Unknown objective group {id} found in offering group {pool.ID}");
                        foreach (var obj in proto.Weights.Keys)
                        {
                            Assert.That(protoMan.TryIndex<EntityPrototype>(obj, out var objProto),
                                $"Unknown objective {obj} found in objective group {id}");
                            Assert.That(objProto.TryGetComponent<ObjectiveComponent>(out _, factory),
                                $"Entity {obj} in objective group {id}");
                        }
                    }
                }
            });
        });

        await pair.CleanReturnAsync();
    }
}
