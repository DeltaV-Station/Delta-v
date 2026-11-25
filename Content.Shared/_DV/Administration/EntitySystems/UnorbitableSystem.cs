using Content.Shared._DV.Administration.Components;
using Content.Shared.Administration.Managers;
using Content.Shared.Follower;
using Content.Shared.Follower.Components;

namespace Content.Shared._DV.Administration.EntitySystems;

public sealed class UnorbitableSystem : EntitySystem
{
    [Dependency] private readonly FollowerSystem _followerSystem = default!;
    [Dependency] private readonly ISharedAdminManager _admin = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UnorbitableComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<UnorbitableComponent> ent, ref ComponentInit args)
    {
        if (!HasComp<FollowedComponent>(ent))
            return;

        _followerSystem.StopAllFollowers(ent);
    }

    public bool CanFollow(EntityUid follower, EntityUid target)
    {
        if (TryComp<UnorbitableComponent>(target, out var unorbitable))
        {
            return unorbitable.AllowAdmins && _admin.IsAdmin(follower);
        }

        return true;
    }
}
