using Content.Shared.Actions;
using Content.Shared.Mind;
using Robust.Shared.Containers;
using Robust.Shared.Network;

namespace Content.Shared._DV.Roles;

public sealed class ActionGrantRoleSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionGrantRoleComponent, EntGotInsertedIntoContainerMessage>(OnGotInserted);
        SubscribeLocalEvent<ActionGrantRoleComponent, EntGotRemovedFromContainerMessage>(OnGotRemoved);
    }

    private void OnGotInserted(Entity<ActionGrantRoleComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (_net.IsClient || !TryComp<MindComponent>(args.Container.Owner, out var mind) || mind.CurrentEntity is not { } mindContainer)
            return;

        _actions.GrantContainedActions(mindContainer, ent.Owner);
    }

    private void OnGotRemoved(Entity<ActionGrantRoleComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (_net.IsClient || !TryComp<MindComponent>(args.Container.Owner, out var mind) || mind.CurrentEntity is not { } mindContainer)
            return;

        _actions.RemoveProvidedActions(mindContainer, ent.Owner);
    }
}
