using Content.Server.Pinpointer;
using Content.Shared._DV.Pager;

namespace Content.Server._DV.Pager;

public sealed class PagerButtonSystem : SharedPagerButtonSystem
{
    [Dependency] private readonly NavMapSystem _navMap = default!;

    protected override string Location(Entity<PagerButtonComponent> ent)
    {
        return _navMap.GetNearestBeaconString(ent.Owner);
    }
}
