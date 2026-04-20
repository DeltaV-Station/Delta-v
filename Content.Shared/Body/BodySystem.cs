using System.Linq;
using Content.Shared.DragDrop;
using Robust.Shared.Containers;

namespace Content.Shared.Body;

public sealed partial class BodySystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private EntityQuery<BodyComponent> _bodyQuery;
    private EntityQuery<OrganComponent> _organQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, ComponentInit>(OnBodyInit);
        SubscribeLocalEvent<BodyComponent, ComponentShutdown>(OnBodyShutdown);

        SubscribeLocalEvent<BodyComponent, CanDragEvent>(OnCanDrag);

        SubscribeLocalEvent<BodyComponent, EntInsertedIntoContainerMessage>(OnBodyEntInserted);
        SubscribeLocalEvent<BodyComponent, EntRemovedFromContainerMessage>(OnBodyEntRemoved);

        SubscribeLocalEvent<BodyComponent, OrganInsertedIntoEvent>(OnOrganInserted);
        SubscribeLocalEvent<BodyComponent, OrganRemovedFromEvent>(OnOrganRemoved);

        _bodyQuery = GetEntityQuery<BodyComponent>();
        _organQuery = GetEntityQuery<OrganComponent>();

        InitializeRelay();
    }

    private void OnBodyInit(Entity<BodyComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Organs =
            _container.EnsureContainer<Container>(ent, BodyComponent.ContainerID);
    }

    private void OnBodyShutdown(Entity<BodyComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Organs is { } organs)
            _container.ShutdownContainer(organs);
    }

    private void OnBodyEntInserted(Entity<BodyComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != BodyComponent.ContainerID)
            return;

        if (!_organQuery.TryComp(args.Entity, out var organ))
            return;

        var body = new OrganInsertedIntoEvent(args.Entity);
        RaiseLocalEvent(ent, ref body);

        var ev = new OrganGotInsertedEvent(ent);
        RaiseLocalEvent(args.Entity, ref ev);

        if (organ.Body != ent)
        {
            organ.Body = ent;
            Dirty(args.Entity, organ);
        }
    }

    private void OnBodyEntRemoved(Entity<BodyComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != BodyComponent.ContainerID)
            return;

        if (!_organQuery.TryComp(args.Entity, out var organ))
            return;

        var body = new OrganRemovedFromEvent(args.Entity);
        RaiseLocalEvent(ent, ref body);

        var ev = new OrganGotRemovedEvent(ent);
        RaiseLocalEvent(args.Entity, ref ev);

        if (organ.Body == null)
            return;

        organ.Body = null;
        Dirty(args.Entity, organ);
    }

    private void OnCanDrag(Entity<BodyComponent> ent, ref CanDragEvent args)
    {
        args.Handled = true;
    }

    // Delta V - Organ Functionality Solution
    private void OnOrganInserted(Entity<BodyComponent> ent, ref OrganInsertedIntoEvent args)
    {
        if (!TryComp<OrganComponent>(args.Organ, out var organ) || organ.onAdd == null)
            return;

        // Delta V - Begin Organ Functionality Solution
        foreach (var (key, comp) in organ.onAdd)
        {
            var compType = comp.Component.GetType();
            if (HasComp(ent, compType))
                continue;

            EntityManager.AddComponent(ent, comp.Component);
        }
    }

    private void OnOrganRemoved(Entity<BodyComponent> ent, ref OrganRemovedFromEvent args)
    {
        if (!TryComp<OrganComponent>(args.Organ, out var organ) || organ.onAdd == null)
            return;

        // Delta V - Begin Organ Functionality Solution
        foreach (var (key, comp) in organ.onAdd)
        {
            var compType = comp.Component.GetType();
            if (!HasComp(ent, compType))
                continue;

            EntityManager.RemoveComponent(ent, comp.Component);
        }
    }
    // Delta V - End
}
