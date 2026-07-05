#nullable enable
using System.Collections.Generic;
using System.Linq;
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Shared._DV.Diona;
using Content.Shared.Gibbing;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Species;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._DV;

[TestFixture]
[TestOf(typeof(DVGestaltSystem))]
[TestOf(typeof(DVNymphingBodySystem))]
[TestOf(typeof(DVNymphRelationSystem))]
public sealed class DVDionaTest : GameTest
{
    private static readonly EntProtoId NymphPrototype = "DVMobDionaNymph";
    private static readonly EntProtoId ReformedPrototype = "MobDionaReformed";

    private static readonly ProtoId<SpeciesPrototype> Diona = "Diona";
    private static readonly Gender Gender = Gender.Epicene;
    public static readonly Sex Sex = Sex.Unsexed;
    public static readonly int Age = 72;
    public static readonly float Height = 1f;

    [SidedDependency(Side.Server)] private readonly SharedMindSystem _mind = null!;
    [SidedDependency(Side.Server)] private readonly DVNymphRelationSystem _relations = null!;
    [SidedDependency(Side.Server)] private readonly GibbingSystem _gibbing = null!;
    [SidedDependency(Side.Server)] private readonly MetaDataSystem _metadata = null!;

    [Test]
    [RunOnSide(Side.Server)]
    public void Assimilation()
    {
        var actor = SSpawn(NymphPrototype);
        var mindlessTarget = SSpawn(NymphPrototype);
        var mindedTarget = SSpawn(NymphPrototype);

        var actorMind = _mind.CreateMind(null);
        _mind.TransferTo(actorMind, actor);

        var targetMind = _mind.CreateMind(null);
        _mind.TransferTo(targetMind, mindedTarget);

        var assimilate = new DVAssimilateNymphActionEvent { Target = mindlessTarget };
        SEntMan.EventBus.RaiseLocalEvent(actor, assimilate);
        Assert.That(assimilate.Handled, Is.True);

        var gestalt = GetSingleGestalt();
        var mapComp = SComp<MapComponent>(gestalt.Comp.NymphStorageMap);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(gestalt.Comp.NymphCount, Is.EqualTo(2));
            Assert.That(gestalt.Comp.StoredNymphs, Is.EquivalentTo([actor, mindlessTarget]));
            Assert.That(SComp<DVGestaltMemberComponent>(actor).StoredInGestalt, Is.EqualTo(gestalt.Owner));
            Assert.That(SComp<DVGestaltMemberComponent>(mindlessTarget).StoredInGestalt, Is.EqualTo(gestalt.Owner));
            Assert.That(SComp<TransformComponent>(actor).MapID, Is.EqualTo(mapComp.MapId));
            Assert.That(SComp<TransformComponent>(mindlessTarget).MapID, Is.EqualTo(mapComp.MapId));
            Assert.That(_mind.GetMind(gestalt), Is.EqualTo(actorMind.Owner));
        }

        var beforeCount = gestalt.Comp.NymphCount;
        var beforeStored = gestalt.Comp.StoredNymphs.Count;
        assimilate = new DVAssimilateNymphActionEvent { Target = mindedTarget };

        SEntMan.EventBus.RaiseLocalEvent(gestalt, assimilate);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(assimilate.Handled, Is.False);
            Assert.That(gestalt.Comp.NymphCount, Is.EqualTo(beforeCount));
            Assert.That(gestalt.Comp.StoredNymphs.Count, Is.EqualTo(beforeStored));
            Assert.That(gestalt.Comp.StoredNymphs, Is.EquivalentTo([actor, mindlessTarget]));
        }
    }

    [Test]
    [RunOnSide(Side.Server)]
    public void GestaltMembership()
    {
        var actor = SSpawn(NymphPrototype);
        var target = SSpawn(NymphPrototype);

        var assimilate = new DVAssimilateNymphActionEvent { Target = target };
        SEntMan.EventBus.RaiseLocalEvent(actor, assimilate);
        Assert.That(assimilate.Handled, Is.True);

        var gestalt = GetSingleGestalt();
        Assert.That(gestalt.Comp.NymphCount, Is.EqualTo(2));

        SEntMan.RemoveComponent<DVGestaltMemberComponent>(target);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(gestalt.Comp.NymphCount, Is.EqualTo(1));
            Assert.That(gestalt.Comp.StoredNymphs, Is.EquivalentTo([actor]));
        }
    }

    [Test]
    [RunOnSide(Side.Server)]
    public void GestaltReformWithDifferentIdentities()
    {
        var first = SpawnProfiledNymph("Mismatched Rings");
        var second = SpawnProfiledNymph("Different Rings");
        var third = SpawnProfiledNymph("Mismatched Rings");

        var assimilate = new DVAssimilateNymphActionEvent { Target = second };
        SEntMan.EventBus.RaiseLocalEvent(first, assimilate);
        Assert.That(assimilate.Handled, Is.True);

        var gestalt = GetSingleGestalt();
        Assert.That(gestalt.Comp.NymphCount, Is.EqualTo(2));

        assimilate = new DVAssimilateNymphActionEvent { Target = third };
        SEntMan.EventBus.RaiseLocalEvent(gestalt, assimilate);
        Assert.That(assimilate.Handled, Is.True);

        var reform = new ReformSystem.ReformEvent();
        SEntMan.EventBus.RaiseLocalEvent(gestalt, reform);
        Assert.That(reform.Handled, Is.True);

        var reformed = FindEntityByPrototype(ReformedPrototype);
        Assert.That(reformed, Is.Not.Null);

        Assert.That(SComp<MetaDataComponent>(reformed.Value).EntityName, Is.Not.EqualTo("Mismatched Rings"));
        Assert.That(SComp<MetaDataComponent>(reformed.Value).EntityName, Is.Not.EqualTo("Different Rings"));
    }

    [Test]
    [RunOnSide(Side.Server)]
    public void GestaltReformWithSameIdentities()
    {
        var first = SpawnProfiledNymph("Identical Rings");
        var second = SpawnProfiledNymph("Identical Rings");
        var third = SpawnProfiledNymph("Identical Rings");

        var firstMind = _mind.CreateMind(null);
        _mind.TransferTo(firstMind, first);

        var assimilate = new DVAssimilateNymphActionEvent { Target = second };
        SEntMan.EventBus.RaiseLocalEvent(first, assimilate);
        Assert.That(assimilate.Handled, Is.True);

        var gestalt = GetSingleGestalt();
        Assert.That(gestalt.Comp.NymphCount, Is.EqualTo(2));

        assimilate = new DVAssimilateNymphActionEvent { Target = third };
        SEntMan.EventBus.RaiseLocalEvent(gestalt, assimilate);
        Assert.That(assimilate.Handled, Is.True);

        var reform = new ReformSystem.ReformEvent();
        SEntMan.EventBus.RaiseLocalEvent(gestalt, reform);
        Assert.That(reform.Handled, Is.True);

        var reformed = FindEntityByPrototype(ReformedPrototype);
        Assert.That(reformed, Is.Not.Null);

        var profile = SComp<HumanoidProfileComponent>(reformed.Value);

        Assert.Multiple(() =>
        {
            Assert.That(_mind.GetMind(reformed.Value), Is.EqualTo(firstMind.Owner));
            Assert.That(SComp<MetaDataComponent>(reformed.Value).EntityName, Is.EqualTo("Identical Rings"));
            Assert.That(profile.Species, Is.EqualTo(Diona));
            Assert.That(profile.Gender, Is.EqualTo(Gender));
            Assert.That(profile.Sex, Is.EqualTo(Sex));
            Assert.That(profile.Age, Is.EqualTo(Age));
            Assert.That(profile.Height, Is.EqualTo(Height));
        });
    }

    [Test]
    [RunOnSide(Side.Server)]
    [NonParallelizable]
    public void GestaltMapPreservation([Range(1, 4)] int additionalNymphCount)
    {
        var first = SpawnProfiledNymph("Identical Rings");
        var others = new List<EntityUid>();
        for (var i = 0; i < additionalNymphCount; i++)
        {
            others.Add(SpawnProfiledNymph("Identical Rings"));
        }

        var second = SpawnProfiledNymph("Identical Rings");

        var assimilate = new DVAssimilateNymphActionEvent { Target = second };
        SEntMan.EventBus.RaiseLocalEvent(first, assimilate);
        Assert.That(assimilate.Handled, Is.True);

        var gestalt = GetSingleGestalt();

        foreach (var other in others)
        {
            assimilate = new DVAssimilateNymphActionEvent { Target = other };
            SEntMan.EventBus.RaiseLocalEvent(gestalt, assimilate);
            Assert.That(assimilate.Handled, Is.True);
        }

        Assert.That(gestalt.Comp.NymphCount, Is.EqualTo(2 + additionalNymphCount));

        var gestaltMap = SComp<MapComponent>(gestalt.Comp.NymphStorageMap);

        foreach (var other in others)
        {
            Assert.That(SComp<TransformComponent>(other).MapID, Is.EqualTo(gestaltMap.MapId));
        }

        var reform = new ReformSystem.ReformEvent();
        SEntMan.EventBus.RaiseLocalEvent(gestalt, reform);
        Assert.That(reform.Handled, Is.True);

        var reformed = FindEntityByPrototype(ReformedPrototype);
        Assert.That(reformed, Is.Not.Null);

        var reformedGestalt = SComp<DVGestaltComponent>(reformed.Value);
        Assert.That(reformedGestalt.NymphCount, Is.EqualTo(2 + additionalNymphCount));

        var reformedGestaltMap = SComp<MapComponent>(reformedGestalt.NymphStorageMap);

        foreach (var other in others)
        {
            Assert.That(SComp<TransformComponent>(other).MapID, Is.EqualTo(reformedGestaltMap.MapId));
        }
    }


    [Test]
    [RunOnSide(Side.Server)]
    public void Relations()
    {
        var leader = SSpawn(NymphPrototype);
        var follower = SSpawn(NymphPrototype);

        _relations.Follow(
            (leader, SComp<DVNymphLeadComponent>(leader)),
            (follower, SComp<DVNymphFollowerComponent>(follower)));

        var leaderComp = SComp<DVNymphLeadComponent>(leader);
        var followerComp = SComp<DVNymphFollowerComponent>(follower);
        var htn = SComp<HTNComponent>(follower);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(leaderComp.Followers, Does.Contain(follower));
            Assert.That(followerComp.Lead, Is.EqualTo(leader));
            Assert.That(htn.Blackboard.TryGetValue<EntityCoordinates>(NPCBlackboard.FollowTarget, out var coords, SEntMan), Is.True);
            Assert.That(coords!.EntityId, Is.EqualTo(leader));
        }

        SEntMan.RemoveComponent<DVNymphLeadComponent>(leader);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(followerComp.Lead, Is.Null);
            Assert.That(htn.Blackboard.TryGetValue<EntityCoordinates>(NPCBlackboard.FollowTarget, out _, SEntMan), Is.False);
        }
    }

    [Test]
    [RunOnSide(Side.Server)]
    public void GibRelations()
    {
        var body = SSpawn("MobDiona");
        _metadata.SetEntityName(body, "Remembered Rings");

        var bodyMind = _mind.CreateMind(null);
        _mind.TransferTo(bodyMind, body);

        var giblets = _gibbing.Gib(body);
        Assert.That(giblets.Count, Is.GreaterThanOrEqualTo(3));

        var nymphs = giblets
            .Where(SEntMan.EntityExists)
            .Where(SEntMan.HasComponent<DVNymphProfileComponent>)
            .ToList();

        Assert.That(nymphs.Count, Is.EqualTo(3));

        var lead = nymphs.Single(uid => _mind.GetMind(uid) == bodyMind);
        var leadComp = SComp<DVNymphLeadComponent>(lead);
        var memory = SComp<DVNymphMindMemoryComponent>(lead);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(memory.Mind, Is.EqualTo(bodyMind.Owner));
            Assert.That(leadComp.Followers, Is.EquivalentTo(nymphs));
        }

        foreach (var nymph in nymphs)
        {
            var profile = SComp<DVNymphProfileComponent>(nymph);
            var follower = SComp<DVNymphFollowerComponent>(nymph);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(profile.Name, Is.EqualTo("Remembered Rings"));
                Assert.That(follower.Lead, Is.EqualTo(lead));
            }
        }
    }

    private EntityUid SpawnProfiledNymph(string name)
    {
        var uid = SSpawn(NymphPrototype);
        var profile = SComp<DVNymphProfileComponent>(uid);

        profile.Name = name;
        profile.Species = Diona;
        profile.Gender = Gender;
        profile.Sex = Sex;
        profile.Age = Age;
        profile.Height = Height;

        return uid;
    }

    private Entity<DVGestaltComponent> GetSingleGestalt()
    {
        EntityUid foundUid = default;
        DVGestaltComponent? foundComp = null;
        var count = 0;
        var query = SEntMan.EntityQueryEnumerator<DVGestaltComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (SEntMan.Deleted(uid))
                continue;

            foundUid = uid;
            foundComp = comp;
            count++;
        }

        Assert.That(count, Is.EqualTo(1));
        return (foundUid, foundComp!);
    }

    private EntityUid? FindEntityByPrototype(EntProtoId prototype)
    {
        var query = SEntMan.EntityQueryEnumerator<MetaDataComponent>();
        while (query.MoveNext(out var uid, out var meta))
        {
            if (SEntMan.Deleted(uid))
                continue;

            if (meta.EntityPrototype?.ID == prototype.Id)
                return uid;
        }

        return null;
    }
}
