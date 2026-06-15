using Content.Shared.Mind;
using Robust.Shared.Containers;
using Robust.Shared.Network;

namespace Content.Shared._DV.Roles;

public sealed class ObjectiveRoleSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ObjectiveRoleComponent, EntGotInsertedIntoContainerMessage>(OnInsertedIntoMind);
        SubscribeLocalEvent<ObjectiveRoleComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInsertedIntoMind(Entity<ObjectiveRoleComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (_net.IsClient)
            return;

        if (!TryComp<MindComponent>(args.Container.Owner, out var mind))
            return;

        foreach (var objective in ent.Comp.Objectives)
        {
            _mind.TryAddObjective(args.Container.Owner, mind, objective);
        }
    }

    private void OnShutdown(Entity<ObjectiveRoleComponent> ent, ref ComponentShutdown args)
    {
        if (_net.IsClient)
            return;

        if (!_container.TryGetContainingContainer(ent.Owner, out var container) ||
            !TryComp<MindComponent>(container.Owner, out var mind))
            return;

        var indices = new List<int>();

        for (var i = mind.Objectives.Count; i >= 0; i--)
        {
            var objective = mind.Objectives[i];
            if (MetaData(objective).EntityPrototype?.ID is { } id && ent.Comp.Objectives.Contains(id))
            {
                indices.Add(i);
            }
        }

        foreach (var index in indices)
        {
            _mind.TryRemoveObjective(container.Owner, mind, index);
        }
    }
}
