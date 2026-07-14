using Content.Shared._DV.Fax;
using Content.Shared.Fax;
using Content.Shared.Paper;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Silicons;

public abstract class SharedStationAiFaxSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly PaperSystem _paper = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiFaxComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<StationAiFaxComponent, FaxPrintedEvent>(OnFaxPrinted);
        Subs.BuiEvents<StationAiFaxComponent>(FaxUiKey.Key,
            subs =>
            {
                subs.Event<StationAiFaxExamineMessage>(OnExamine);
                subs.Event<StationAiFaxShredMessage>(OnShred);
            });
    }

    protected virtual void OnInserted(Entity<StationAiFaxComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (HasComp<PaperComponent>(args.Entity))
            EnsureComp<StationAiWhitelistComponent>(args.Entity);
    }

    private void OnExamine(Entity<StationAiFaxComponent> ent, ref StationAiFaxExamineMessage args)
    {
        if (GetDocument(ent, args.Document) is not { } document)
            return;

        if (TryComp<PaperComponent>(document, out var paper))
        {
            paper.Mode = paper.EditingDisabled || paper.StampedBy.Count != 0
                ? PaperComponent.PaperAction.Read
                : PaperComponent.PaperAction.Write;

            _paper.UpdateUserInterface((document, paper));
        }

        _userInterface.OpenUi(document, PaperComponent.PaperUiKey.Key, args.Actor);
    }

    private void OnShred(Entity<StationAiFaxComponent> ent, ref StationAiFaxShredMessage args)
    {
        if (GetDocument(ent, args.Document) is { } document)
        {
            PredictedQueueDel(document);
        }
    }

    protected EntityUid? GetDocument(Entity<StationAiFaxComponent> ent, NetEntity netEntity)
    {
        if (GetEntity(netEntity) is not { Valid: true } entity)
            return null;

        if (!_container.TryGetContainingContainer(entity, out var container))
            return null;

        if (container.Owner != ent.Owner)
            return null;

        return entity;
    }

    private void OnFaxPrinted(Entity<StationAiFaxComponent> ent, ref FaxPrintedEvent args)
    {
        var container = _container.EnsureContainer<Container>(ent, StationAiFaxComponent.Container);
        if (!_container.Insert(args.Fax, container, force: true))
        {
            PredictedQueueDel(args.Fax);
        }
    }
}
