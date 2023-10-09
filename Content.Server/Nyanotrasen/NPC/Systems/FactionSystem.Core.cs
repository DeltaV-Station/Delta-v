using Content.Server.NPC.Components;

namespace Content.Server.NPC.Systems;

public partial class NpcFactionSystem : EntitySystem
{
    public void InitializeCore()
    {
        SubscribeLocalEvent<NpcFactionMemberComponent, GetNearbyHostilesEvent>(OnGetNearbyHostiles);
    }

    public bool ContainsFaction(EntityUid uid, string faction, NpcFactionMemberComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        return component.Factions.Contains(faction);
    }

    public void AddFriendlyEntity(EntityUid uid, EntityUid fEntity, NpcFactionMemberComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        component.ExceptionalFriendlies.Add(fEntity);
    }

    private void OnGetNearbyHostiles(EntityUid uid, NpcFactionMemberComponent component, ref GetNearbyHostilesEvent args)
    {
        args.ExceptionalFriendlies.UnionWith(component.ExceptionalFriendlies);
    }
}

/// <summary>
/// Raised on an entity when it's trying to determine which nearby entities are hostile.
/// </summary>
/// <param name="ExceptionalHostiles">Entities that will be counted as hostile regardless of faction. Overriden by friendlies.</param>
/// <param name="ExceptionalFriendlies">Entities that will be counted as friendly regardless of faction. Overrides hostiles. </param>
[ByRefEvent]
public readonly record struct GetNearbyHostilesEvent(HashSet<EntityUid> ExceptionalHostiles, HashSet<EntityUid> ExceptionalFriendlies);
