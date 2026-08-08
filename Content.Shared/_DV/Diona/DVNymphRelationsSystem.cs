using JetBrains.Annotations;

namespace Content.Shared._DV.Diona;

public sealed class DVNymphRelationSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVNymphLeadComponent, ComponentShutdown>(OnLeadShutdown);
        SubscribeLocalEvent<DVNymphFollowerComponent, ComponentShutdown>(OnFollowerShutdown);
    }

    private void OnFollowerShutdown(Entity<DVNymphFollowerComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Lead is not { } nymph || !TryComp<DVNymphLeadComponent>(nymph, out var lead))
            return;

        lead.Followers.Remove(ent);
        Dirty(nymph, lead);
    }

    private void OnLeadShutdown(Entity<DVNymphLeadComponent> ent, ref ComponentShutdown args)
    {
        foreach (var nymph in ent.Comp.Followers)
        {
            if (!TryComp<DVNymphFollowerComponent>(nymph, out var follower))
                continue;

            follower.Lead = null;
            Dirty(nymph, follower);

            var evt = new DVNymphFollowerLeadGotChangedEvent();
            RaiseLocalEvent(nymph, ref evt);
        }
    }

    [PublicAPI]
    public void Follow(Entity<DVNymphLeadComponent?> leader, Entity<DVNymphFollowerComponent?> follower)
    {
        if (!Resolve(leader, ref leader.Comp) || !Resolve(follower, ref follower.Comp))
            return;

        leader.Comp.Followers.Add(follower);
        follower.Comp.Lead = leader;

        var evt = new DVNymphFollowerLeadGotChangedEvent();
        RaiseLocalEvent(follower, ref evt);

        Dirty(leader, leader.Comp);
        Dirty(follower, follower.Comp);
    }
}
