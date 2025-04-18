using System.Linq;
using Content.Shared._DV.Polymorph;
using Content.Shared.Mind.Components;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Containers;

public sealed class ContentContainerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ContainerManagerComponent, BeforePolymorphedEvent>(OnBeforePolymorphed);

        base.Initialize();
    }

    private void OnBeforePolymorphed(Entity<ContainerManagerComponent> ent, ref BeforePolymorphedEvent args)
    {
        // Recursively drop all entities with MindContainerComponent in all containers on the entity
        // This prevents players from being sent to null-space when carried by a polymorphing entity
        var stack = new Stack<EntityUid>();
        stack.Push(ent);

        while (stack.Count > 0)
        {
            var currentUid = stack.Pop();

            if (!HasComp<ContainerManagerComponent>(currentUid))
                continue;

            foreach (var container in _container.GetAllContainers(currentUid).ToList())
            {
                foreach (var entity in container.ContainedEntities.ToList())
                {
                    if (HasComp<MindContainerComponent>(entity))
                    {
                        _transform.AttachToGridOrMap(entity);
                        continue;
                    }

                    if (stack.Count < 1000) // Unlikely to have over 1000 nested entities unless there is an infinite loop.
                        stack.Push(entity); // Process this entity's containers next
                }
            }
        }

    }
}
