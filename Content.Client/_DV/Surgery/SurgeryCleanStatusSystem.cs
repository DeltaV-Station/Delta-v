using Content.Client.Items;
using Content.Shared._DV.Surgery;
using Content.Shared.Inventory;
using Robust.Shared.Containers;

namespace Content.Client._DV.Surgery;

/// <summary>
/// This shows the item status for dirty surgery tools.
/// </summary>
public sealed class SurgeryCleanStatusSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<SurgeryDirtinessComponent>(ent => new SurgeryDirtinessItemStatus(ent, EntityManager, _inventory, _container));
    }
}
