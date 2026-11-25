using Content.Shared._DV.Silicons;
using Content.Shared.Fax;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;

namespace Content.Client._DV.Silicons;

public sealed class StationAiFaxSystem : SharedStationAiFaxSystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiFaxComponent, EntRemovedFromContainerMessage>(OnRemoved);
    }

    protected override void OnInserted(Entity<StationAiFaxComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        base.OnInserted(ent, ref args);

        if (!_userInterface.TryGetOpenUi<StationAiFaxBoundUserInterface>(ent.Owner, FaxUiKey.Key, out var bui))
            return;

        bui.UpdateFaxes(_container.EnsureContainer<Container>(ent, StationAiFaxComponent.Container, out _));
    }

    private void OnRemoved(Entity<StationAiFaxComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!_userInterface.TryGetOpenUi<StationAiFaxBoundUserInterface>(ent.Owner, FaxUiKey.Key, out var bui))
            return;

        bui.UpdateFaxes(_container.EnsureContainer<Container>(ent, StationAiFaxComponent.Container, out _));
    }
}
