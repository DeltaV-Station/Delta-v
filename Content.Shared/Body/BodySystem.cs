using System.Linq;
using Content.Shared.DragDrop;
using Robust.Shared.Containers;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared.Body;

public sealed partial class BodySystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;

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

        SubscribeLocalEvent<BodyComponent, OrganInsertedIntoEvent>(OnOrganInserted); // Delta V - Nubody Organ Functionality Workaround

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
        if (!TryComp<OrganComponent>(args.Organ, out var organ) || organ.OnAdd == null)
            return;

        foreach (var (key, comp) in organ.OnAdd)
        {
            var compType = comp.Component.GetType();
            if (HasComp(ent, compType))
                continue;

            var newComp = _serializationManager.CreateCopy(comp.Component, notNullableOverride: true);
            newComp.Owner = ent;
            EntityManager.AddComponent(ent.Owner, newComp, true);
        }
    }
    // Delta V - End
}
