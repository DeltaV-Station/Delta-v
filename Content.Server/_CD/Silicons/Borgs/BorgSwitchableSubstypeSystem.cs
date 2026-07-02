using Content.Shared._CD.Silicons.Borgs;
using Content.Shared.Inventory;

namespace Content.Server._CD.Silicons.Borgs;

/// <summary>
/// Server-side logic that shouldn't be exposed to the client.
/// </summary>
public sealed class BorgSwitchableSubstypeSystem : SharedBorgSwitchableSubtypeSystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    protected override void SelectBorgSubtype(Entity<BorgSwitchableSubtypeComponent> ent)
    {
        if (ent.Comp.BorgSubtype == null)
            return;

        if (!Prototypes.Index(ent.Comp.BorgSubtype.Value)
            .TryGetComponent<BorgSubtypeDefinitionComponent>(out var borgSubtype, ComponentFactory))
            return;

        // Configure special components
        if (Prototypes.TryIndex(ent.Comp.BorgSubtype, out var previousPrototype) &&
            previousPrototype.TryGetComponent<BorgSubtypeDefinitionComponent>(out var previousSubtype, ComponentFactory))
        {
            if (previousSubtype.AddComponents is { } removeComponents)
                EntityManager.RemoveComponents(ent, removeComponents);
        }

        if (borgSubtype.AddComponents is { } addComponents)
        {
            EntityManager.AddComponents(ent, addComponents);
        }

        // inventory template configuration (hats spacing)
        if (TryComp(ent, out InventoryComponent? inventory))
        {
            _inventorySystem.SetTemplateId((ent.Owner, inventory), borgSubtype.InventoryTemplateId);
        }

        base.SelectBorgSubtype(ent);
    }
}
