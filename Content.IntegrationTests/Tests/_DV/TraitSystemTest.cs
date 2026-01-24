using System.Collections.Generic;
using System.Reflection;
using Content.Server._DV.Traits;
using Content.Shared._DV.Traits;
using Content.Shared._DV.Traits.Conditions;
using Content.Shared._DV.Traits.Effects;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Nutrition.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._DV;

/// <summary>
/// Comprehensive integration tests for the trait system.
/// Tests all conditions, effects, and validation logic.
/// </summary>
[TestFixture]
[TestOf(typeof(TraitSystemTest))]
public sealed partial class TraitSystemTest
{
    [TestPrototypes]
    private const string Prototypes = @"
# Test Trait Categories
- type: traitCategory
  id: TestCategoryUnlimited
  name: trait-dysgraphia-name
  maxTraits: null
  maxPoints: null

- type: traitCategory
  id: TestCategoryLimited
  name: trait-dysgraphia-name
  maxTraits: 2
  maxPoints: 10

# Test Traits - Conditions
- type: trait
  id: TestTraitHasComp
  name: trait-dysgraphia-name
  description: trait-dysgraphia-name
  category: TestCategoryUnlimited
  cost: 0
  conditions:
  - !type:HasCompCondition
    component: Hunger
  effects:
  - !type:AddCompsEffect
    components:
    - type: Test


# Test Traits - Effects
- type: trait
  id: TestTraitAddComps
  name: trait-dysgraphia-name
  description: trait-dysgraphia-name
  category: TestCategoryUnlimited
  cost: 0
  effects:
  - !type:AddCompsEffect
    components:
    - type: Test
    - type: Hunger

- type: trait
  id: TestTraitOverrideComps
  name: trait-dysgraphia-name
  description: trait-dysgraphia-name
  category: TestCategoryUnlimited
  cost: 0
  effects:
  - !type:OverrideCompsEffect
    components:
    - type: Hunger

- type: trait
  id: TestTraitRemComps
  name: trait-dysgraphia-name
  description: trait-dysgraphia-name
  category: TestCategoryUnlimited
  cost: 0
  effects:
  - !type:RemCompsEffect
    components:
    - Hunger
    - Thirst

- type: trait
  id: TestTraitSpawnItem
  name: trait-dysgraphia-name
  description: trait-dysgraphia-name
  category: TestCategoryUnlimited
  cost: 0
  effects:
  - !type:SpawnItemInHandEffect
    item: Pen

# Test Traits - Validation
- type: trait
  id: TestTraitConflictA
  name: trait-dysgraphia-name
  description: trait-dysgraphia-name
  category: TestCategoryUnlimited
  cost: 0
  conflicts:
  - TestTraitConflictB
  effects:
  - !type:AddCompsEffect
    components:
    - type: Test

- type: trait
  id: TestTraitConflictB
  name: trait-dysgraphia-name
  description: trait-dysgraphia-name
  category: TestCategoryUnlimited
  cost: 0
  effects:
  - !type:AddCompsEffect
    components:
    - type: Test

- type: trait
  id: TestTraitLimited1
  name: trait-dysgraphia-name
  description: trait-dysgraphia-name
  category: TestCategoryLimited
  cost: 5
  effects:
  - !type:AddCompsEffect
    components:
    - type: Test

- type: trait
  id: TestTraitLimited2
  name: trait-dysgraphia-name
  description: trait-dysgraphia-name
  category: TestCategoryLimited
  cost: 5
  effects:
  - !type:AddCompsEffect
    components:
    - type: Test

- type: trait
  id: TestTraitLimited3
  name: trait-dysgraphia-name
  description: trait-dysgraphia-name
  category: TestCategoryLimited
  cost: 5
  effects:
  - !type:AddCompsEffect
    components:
    - type: Test
";

    #region Condition Tests

    [RegisterComponent]
    private sealed partial class TestComponent : Component;

    [Test]
    public async Task HasCompCondition_WithComponent_ReturnsTrue()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);
            entMan.AddComponent<HungerComponent>(player);

            var condition = new HasCompCondition { Component = "Hunger" };
            var ctx = CreateContext(entMan, protoMan, factory, player);

            Assert.That(condition.Evaluate(ctx), Is.True, "HasCompCondition should return true when component exists");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task HasCompCondition_WithoutComponent_ReturnsFalse()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);

            var condition = new HasCompCondition { Component = "Hunger" };
            var ctx = CreateContext(entMan, protoMan, factory, player);

            Assert.That(condition.Evaluate(ctx),
                Is.False,
                "HasCompCondition should return false when component doesn't exist");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task HasCompCondition_Inverted_ReturnsOpposite()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);
            entMan.AddComponent<HungerComponent>(player);

            var condition = new HasCompCondition { Component = "Hunger", Invert = true };
            var ctx = CreateContext(entMan, protoMan, factory, player);

            Assert.That(condition.Evaluate(ctx),
                Is.False,
                "Inverted HasCompCondition should return false when component exists");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task HasJobCondition_MatchingJob_ReturnsTrue()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);

            var condition = new HasJobCondition { Job = "MedicalDoctor" };
            var ctx = CreateContext(entMan, protoMan, factory, player, "MedicalDoctor");

            Assert.That(condition.Evaluate(ctx), Is.True, "HasJobCondition should return true for matching job");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task HasJobCondition_DifferentJob_ReturnsFalse()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);

            var condition = new HasJobCondition { Job = "MedicalDoctor" };
            var ctx = CreateContext(entMan, protoMan, factory, player, "SecurityOfficer");

            Assert.That(condition.Evaluate(ctx), Is.False, "HasJobCondition should return false for different job");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task InDepartmentCondition_JobInDepartment_ReturnsTrue()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);

            var condition = new InDepartmentCondition { Department = "Medical" };
            var ctx = CreateContext(entMan, protoMan, factory, player, "MedicalDoctor");

            Assert.That(condition.Evaluate(ctx),
                Is.True,
                "InDepartmentCondition should return true when job is in department");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task InDepartmentCondition_JobNotInDepartment_ReturnsFalse()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);

            var condition = new InDepartmentCondition { Department = "Medical" };
            var ctx = CreateContext(entMan, protoMan, factory, player, "SecurityOfficer");

            Assert.That(condition.Evaluate(ctx),
                Is.False,
                "InDepartmentCondition should return false when job is not in department");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task IsSpeciesCondition_MatchingSpecies_ReturnsTrue()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);

            var condition = new IsSpeciesCondition { Species = "Human" };
            var ctx = CreateContext(entMan, protoMan, factory, player, speciesId: "Human");

            Assert.That(condition.Evaluate(ctx), Is.True, "IsSpeciesCondition should return true for matching species");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task IsSpeciesCondition_DifferentSpecies_ReturnsFalse()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);

            var condition = new IsSpeciesCondition { Species = "Human" };
            var ctx = CreateContext(entMan, protoMan, factory, player, speciesId: "Vox");

            Assert.That(condition.Evaluate(ctx),
                Is.False,
                "IsSpeciesCondition should return false for different species");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    #endregion

    #region Effect Tests

    [Test]
    public async Task AddCompsEffect_AddsComponents()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);
            Assert.That(entMan.HasComponent<HungerComponent>(player),
                Is.False,
                "Player should not start with HungerComponent");

            var trait = protoMan.Index(new ProtoId<TraitPrototype>("TestTraitAddComps"));
            var ctx = CreateEffectContext(entMan, protoMan, factory, player);

            foreach (var effect in trait.Effects)
            {
                effect.Apply(ctx);
            }

            Assert.That(entMan.HasComponent<HungerComponent>(player),
                Is.True,
                "AddCompsEffect should add HungerComponent");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task AddCompsEffect_DoesNotOverwrite()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);
            var hungerBefore = entMan.AddComponent<HungerComponent>(player);

            var trait = protoMan.Index(new ProtoId<TraitPrototype>("TestTraitAddComps"));
            var ctx = CreateEffectContext(entMan, protoMan, factory, player);

            foreach (var effect in trait.Effects)
            {
                effect.Apply(ctx);
            }

            var hungerAfter = entMan.GetComponent<HungerComponent>(player);
            Assert.That(hungerAfter,
                Is.SameAs(hungerBefore),
                "AddCompsEffect should not replace existing component instance");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task OverrideCompsEffect_OverwritesComponent()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);
            var hungerBefore = entMan.AddComponent<HungerComponent>(player);

            var trait = protoMan.Index(new ProtoId<TraitPrototype>("TestTraitOverrideComps"));
            var ctx = CreateEffectContext(entMan, protoMan, factory, player);

            foreach (var effect in trait.Effects)
            {
                effect.Apply(ctx);
            }

            var hungerAfter = entMan.GetComponent<HungerComponent>(player);
            Assert.That(hungerAfter,
                Is.Not.SameAs(hungerBefore),
                "OverrideCompsEffect should replace existing component instance");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task RemCompsEffect_RemovesComponents()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);
            entMan.AddComponent<HungerComponent>(player);
            entMan.AddComponent<ThirstComponent>(player);

            Assert.That(entMan.HasComponent<HungerComponent>(player),
                Is.True,
                "Player should start with HungerComponent");
            Assert.That(entMan.HasComponent<ThirstComponent>(player),
                Is.True,
                "Player should start with ThirstComponent");

            var trait = protoMan.Index(new ProtoId<TraitPrototype>("TestTraitRemComps"));
            var ctx = CreateEffectContext(entMan, protoMan, factory, player);

            foreach (var effect in trait.Effects)
            {
                effect.Apply(ctx);
            }

            Assert.That(entMan.HasComponent<HungerComponent>(player),
                Is.False,
                "RemCompsEffect should remove HungerComponent");
            Assert.That(entMan.HasComponent<ThirstComponent>(player),
                Is.False,
                "RemCompsEffect should remove ThirstComponent");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task SpawnItemInHandEffect_SpawnsItem()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity("MobHuman", MapCoordinates.Nullspace);
            var handsSys = entMan.System<SharedHandsSystem>();
            var hands = entMan.GetComponent<HandsComponent>(player);

            Assert.That(handsSys.GetActiveItem((player, hands)), Is.Null, "Player should start with empty hands");

            var traitSys = entMan.System<TraitSystem>();
            var trait = protoMan.Index(new ProtoId<TraitPrototype>("TestTraitSpawnItem"));

            // We need to use reflection to call the private ApplyTrait method
            var method = typeof(TraitSystem).GetMethod("ApplyTrait",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(traitSys, new object[] { player, trait });

            var item = handsSys.GetActiveItem((player, hands));
            Assert.That(item, Is.Not.Null, "SpawnItemInHandEffect should spawn item in hand");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    #endregion

    #region Validation Tests

    [Test]
    public async Task RespectsConflicts()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);

            var selectedTraits = new HashSet<ProtoId<TraitPrototype>>
            {
                "TestTraitConflictA",
                "TestTraitConflictB",
            };

            var traitSys = entMan.System<TraitSystem>();
            var method = typeof(TraitSystem).GetMethod("ValidateTraits",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var validTraits = (HashSet<ProtoId<TraitPrototype>>)method?.Invoke(traitSys,
                new object[] { player, selectedTraits, null, null, null });

            Assert.Multiple(() =>
            {
                Assert.That(validTraits?.Count, Is.EqualTo(1), "Only one conflicting trait should be valid");
                Assert.That(validTraits.Contains("TestTraitConflictA"), Is.True, "First trait should be kept");
                Assert.That(validTraits.Contains("TestTraitConflictB"),
                    Is.False,
                    "Conflicting trait should be rejected");
            });

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task RespectsCategoryLimits()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);

            // TestCategoryLimited has maxTraits: 2
            var selectedTraits = new HashSet<ProtoId<TraitPrototype>>
            {
                "TestTraitLimited1",
                "TestTraitLimited2",
                "TestTraitLimited3",
            };

            var traitSys = entMan.System<TraitSystem>();
            var method = typeof(TraitSystem).GetMethod("ValidateTraits",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var validTraits = (HashSet<ProtoId<TraitPrototype>>)method?.Invoke(traitSys,
                new object[] { player, selectedTraits, null, null, null });

            Assert.That(validTraits?.Count, Is.EqualTo(2), "Should respect category maxTraits limit");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task RespectsCategoryPointLimits()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);

            // TestCategoryLimited has maxPoints: 10, each trait costs 5
            var selectedTraits = new HashSet<ProtoId<TraitPrototype>>
            {
                "TestTraitLimited1",
                "TestTraitLimited2",
                "TestTraitLimited3", // This would exceed the 10 point limit
            };

            var traitSys = entMan.System<TraitSystem>();
            var method = typeof(TraitSystem).GetMethod("ValidateTraits",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var validTraits = (HashSet<ProtoId<TraitPrototype>>)method?.Invoke(traitSys,
                new object[] { player, selectedTraits, null, null, null });

            Assert.That(validTraits?.Count, Is.EqualTo(2), "Should respect category maxPoints limit");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task ChecksConditionsOnSpawn()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);
            entMan.AddComponent<HungerComponent>(player);

            // Trait requires HungerComponent
            var selectedTraits = new HashSet<ProtoId<TraitPrototype>>
            {
                "TestTraitHasComp",
            };

            var traitSys = entMan.System<TraitSystem>();
            var method = typeof(TraitSystem).GetMethod("ValidateTraits",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var validTraits = (HashSet<ProtoId<TraitPrototype>>)method?.Invoke(traitSys,
                new object[] { player, selectedTraits, null, null, null });

            Assert.That(validTraits?.Contains("TestTraitHasComp"), Is.True, "Trait with met condition should be valid");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task RejectsTraitsWithUnmetConditions()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();

        await server.WaitAssertion(() =>
        {
            var player = entMan.SpawnEntity(null, MapCoordinates.Nullspace);
            // Player does NOT have HungerComponent

            // Trait requires HungerComponent
            var selectedTraits = new HashSet<ProtoId<TraitPrototype>>
            {
                "TestTraitHasComp",
            };

            var traitSys = entMan.System<TraitSystem>();
            var method = typeof(TraitSystem).GetMethod("ValidateTraits",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var validTraits = (HashSet<ProtoId<TraitPrototype>>)method?.Invoke(traitSys,
                new object[] { player, selectedTraits, null, null, null });

            Assert.That(validTraits?.Contains("TestTraitHasComp"),
                Is.False,
                "Trait with unmet condition should be rejected");

            entMan.DeleteEntity(player);
        });

        await pair.CleanReturnAsync();
    }

    #endregion

    #region Helper Methods

    private static TraitConditionContext CreateContext(
        IEntityManager entMan,
        IPrototypeManager protoMan,
        IComponentFactory factory,
        EntityUid player,
        string? jobId = null,
        string? speciesId = null)
    {
        return new TraitConditionContext
        {
            Player = player,
            Session = null,
            EntMan = entMan,
            Proto = protoMan,
            CompFactory = factory,
            LogMan = IoCManager.Resolve<ILogManager>(),
            JobId = jobId,
            SpeciesId = speciesId,
        };
    }

    private static TraitEffectContext CreateEffectContext(
        IEntityManager entMan,
        IPrototypeManager protoMan,
        IComponentFactory factory,
        EntityUid player)
    {
        return new TraitEffectContext
        {
            Player = player,
            EntMan = entMan,
            Proto = protoMan,
            CompFactory = factory,
            LogMan = IoCManager.Resolve<ILogManager>(),
            Transform = entMan.GetComponent<TransformComponent>(player),
        };
    }

    #endregion
}
