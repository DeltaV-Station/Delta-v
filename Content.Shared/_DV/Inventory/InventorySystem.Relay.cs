using Content.Shared._DV.Chemistry.Systems;

namespace Content.Shared.Inventory;

public partial class InventorySystem
{
    public void InitializeRelayDV()
    {
        // by-ref events
        SubscribeLocalEvent<InventoryComponent, SafeSolutionThrowEvent>(RefRelayInventoryEvent);
    }
}
