using System.Numerics;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared._DV.Diona;
using Robust.Shared.Map;

namespace Content.Server._DV.Diona;

public sealed class DVNymphNPCSystem : EntitySystem
{
    [Dependency] private readonly NPCSystem _npc = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVNymphFollowerComponent, DVNymphFollowerLeadGotChangedEvent>(OnLeadGotChanged);
    }

    private void OnLeadGotChanged(Entity<DVNymphFollowerComponent> ent, ref DVNymphFollowerLeadGotChangedEvent args)
    {
        if (ent.Comp.Lead is { } leader)
            _npc.SetBlackboard(ent, NPCBlackboard.FollowTarget, new EntityCoordinates(leader, Vector2.Zero));
        else if (TryComp<HTNComponent>(ent, out var htn))
        {
            htn.Blackboard.Remove<EntityCoordinates>(NPCBlackboard.FollowTarget);
        }
    }
}
