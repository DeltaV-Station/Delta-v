using Robust.Shared.GameStates;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._DV.ShadowWalk;

public abstract class SharedShadowWalkSystem : EntitySystem
{
    [Dependency] protected readonly SharedPhysicsSystem Physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowWalkerComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<ShadowWalkerComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
    }

    private void OnPreventCollide(Entity<ShadowWalkerComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
            return;

        // Only phase through hard blockers; sensor fixtures must keep triggering.
        if (!args.OurFixture.Hard || !args.OtherFixture.Hard)
            return;

        if (ent.Comp.PassableEntities.Contains(args.OtherEntity))
            args.Cancelled = true;
    }

    private void OnAfterHandleState(Entity<ShadowWalkerComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        // States are re-applied every prediction reset; only rebuild contacts when the
        // passable set actually changed, since they were filtered with the old one.
        if (ent.Comp.PassableEntities.SetEquals(ent.Comp.LastAppliedPassable))
            return;

        ent.Comp.LastAppliedPassable.Clear();
        ent.Comp.LastAppliedPassable.UnionWith(ent.Comp.PassableEntities);
        Physics.RegenerateContacts(ent.Owner);
    }

    protected void SetPassable(Entity<ShadowWalkerComponent> ent, HashSet<EntityUid> passable)
    {
        ent.Comp.PassableEntities.Clear();
        ent.Comp.PassableEntities.UnionWith(passable);
        Dirty(ent);
        Physics.RegenerateContacts(ent.Owner);
    }
}
