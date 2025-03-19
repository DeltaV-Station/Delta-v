using Content.Client.Items;
using Content.Shared._DV.Surgery;
using Content.Shared.Inventory;
using Robust.Shared.Containers;

namespace Content.Client._DV.Surgery;

/// <summary>
///     This gets the examine tooltip and sanitize verb predicted on the client so there's no pop-in after latency
/// </summary>
public sealed class SurgeryCleanTooltipSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<SurgeryDirtinessComponent>(ent => new SurgeryDirtinessItemStatus(ent, EntityManager, _inventory, _container));
    }
}
