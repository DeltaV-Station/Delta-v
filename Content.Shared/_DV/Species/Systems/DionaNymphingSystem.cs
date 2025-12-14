using Content.Shared.Body.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Species.Components;

namespace Content.Shared._DV.Species.Systems;

/// <summary>
/// Handles item dropping for diona nymphing to prevent items from being deleted during the gib transformation.
/// </summary>
public sealed class DionaNymphingSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Subscribe to BeingGibbedEvent which is raised before the body is deleted
        // This only triggers for entities with GibActionComponent (dionas)
        SubscribeLocalEvent<GibActionComponent, BeingGibbedEvent>(OnBeingGibbed);
    }

    private void OnBeingGibbed(EntityUid uid, GibActionComponent comp, ref BeingGibbedEvent args)
    {
        // Drop all items before the body is deleted to prevent item loss during nymphing
        var xform = Transform(uid);

        // Drop all inventory items
        if (TryComp<InventoryComponent>(uid, out var inventory))
        {
            foreach (var item in _inventory.GetHandOrInventoryEntities(uid))
            {
                _transform.DropNextTo(item, (uid, xform));
            }
        }

        // Drop anything in hands that wasn't caught by inventory
        if (TryComp<HandsComponent>(uid, out var hands))
        {
            foreach (var handName in _hands.EnumerateHands((uid, hands)))
            {
                _hands.TryDrop((uid, hands), handName, checkActionBlocker: false);
            }
        }
    }
}
