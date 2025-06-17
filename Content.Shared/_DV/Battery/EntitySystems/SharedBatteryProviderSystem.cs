using System.Diagnostics.CodeAnalysis;
using Content.Shared._DV.Battery.Components;
using Content.Shared._DV.Battery.Events;
using Content.Shared.Clothing;
using Content.Shared.Inventory;

namespace Content.Shared._DV.Battery.EntitySystems;

/// <summary>
/// Shared code for handling battery provider components.
/// </summary>
public abstract partial class SharedBatteryProviderSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryProviderComponent, ClothingGotEquippedEvent>(OnProviderEquipped);
        SubscribeLocalEvent<BatteryProviderComponent, ClothingGotUnequippedEvent>(OnProviderUnequipped);
    }

    /// <summary>
    /// Attaches a piece of equipment to a battery provider, ensuring it can draw power from any
    /// connected battery.
    /// </summary>
    /// <param name="wearer">The wearer/user of the equipment.</param>
    /// <param name="equipment">The equipment to add.</param>
    /// <returns>True if the equipment was newly connected, false otherwise.</returns>
    public bool AddConnectedEquipment(EntityUid wearer, EntityUid equipment)
    {
        if (!TryFindProvider(wearer, out var suit))
            return false;

        return AddConnectedEquipment((wearer, suit), equipment);
    }

    /// <summary>
    /// Attaches a piece of equipment to a battery provider, ensuring it can draw power from any
    /// connected battery.
    /// </summary>
    /// <param name="ent">The battery provider worn by an entity.</param>
    /// <param name="equipment">The equipment to add.</param>
    /// <returns>True if the equipment was newly connected, false otherwise.</returns>
    public bool AddConnectedEquipment(Entity<BatteryProviderComponent?> ent, EntityUid equipment)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        return ent.Comp.ConnectedEquipment.Add(equipment);
    }

    /// <summary>
    /// Removes a piece of equipment from a battery provider.
    /// </summary>
    /// <param name="wearer">The wearer/user of the equipment.</param>
    /// <param name="equipment">The equipment to remove.</param>
    /// <returns>True if the equipment was found and removed, false otherwise.</returns>
    public bool RemoveConnectedEquipment(EntityUid wearer, EntityUid equipment)
    {
        if (!TryFindProvider(wearer, out var suit))
            return false;

        return RemoveConnectedEquipment((wearer, suit), equipment);
    }


    /// <summary>
    /// Removes a piece of equipment from a battery provider.
    /// </summary>
    /// <param name="ent">The battery provider worn by an entity.</param>
    /// <param name="equipment">The equipment to remove.</param>
    /// <returns>True if the equipment was found and removed, false otherwise.</returns>
    public bool RemoveConnectedEquipment(Entity<BatteryProviderComponent?> ent, EntityUid equipment)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        return ent.Comp.ConnectedEquipment.Remove(equipment);
    }

    /// <summary>
    /// Attempts to find a valid battery provider worn by an entity.
    /// Iterates over all hand and inventory entities attached to the wearer, returns the first found
    /// if the wearer has multiple.
    /// </summary>
    /// <param name="wearer">The wearer to iterate over.</param>
    /// <param name="provider">Out param for the found provider, if any.</param>
    /// <returns>True if a provider was found, false otherwise.</returns>
    private bool TryFindProvider(EntityUid wearer, [NotNullWhen(true)] out BatteryProviderComponent? provider)
    {
        provider = null; // Safety first, null it out

        foreach (var item in _inventorySystem.GetHandOrInventoryEntities(wearer))
        {
            if (TryComp(item, out provider))
                return true; // First thing that provides battery in our list
        }

        return false;
    }

    /// <summary>
    /// Handles when a battery provider is equipped by a wearer.
    /// Raises events over all currently held/worn items by the wearer, giving them a chance to connect.
    /// Also raises events at the user, in case of implants or other in-built systems.
    /// </summary>
    /// <param name="provider">The provider that was just equipped.</param>
    /// <param name="args">Args for the event, notably the wearer.</param>
    private void OnProviderEquipped(Entity<BatteryProviderComponent> provider, ref ClothingGotEquippedEvent args)
    {
        provider.Comp.Wearer = args.Wearer;

        // Provider was equipped, give equipment and items in hand a chance to connect to it via this event.
        var ev = new BatteryProviderEquippedEvent(provider.Owner, []);
        foreach (var item in _inventorySystem.GetHandOrInventoryEntities(args.Wearer))
        {
            RaiseLocalEvent(item, ref ev);
        }
        RaiseLocalEvent(args.Wearer, ref ev); // Remember to inform the wearer too

        // Connect any equipment that's been found.
        provider.Comp.ConnectedEquipment.UnionWith(ev.ConnectedEquipment);
    }

    /// <summary>
    /// Handles when a battery provider is un-equipped by a wearer.
    /// Raises events over all currently held/worn items by the wearer, giving them a chance to gracefully disconnect.
    /// Also raises events at the user, in case of implants or other in-built systems.
    /// </summary>
    /// <param name="provider">The provider that was just un-equipped.</param>
    /// <param name="args">Args for the event, notably the wearer.</param>
    protected virtual void OnProviderUnequipped(Entity<BatteryProviderComponent> provider, ref ClothingGotUnequippedEvent args)
    {
        if (!provider.Comp.Wearer.HasValue || args.Wearer != provider.Comp.Wearer)
            return; // Somehow weren't being worn, or got an event for someone else?

        // Inform equipment that the provider is gone.
        var ev = new BatteryProviderUnequippedEvent(provider);
        foreach (var equipment in provider.Comp.ConnectedEquipment)
        {
            RaiseLocalEvent(equipment, ref ev);
        }
        RaiseLocalEvent(args.Wearer, ref ev); // Remember to inform the wearer too.

        // Cleanup components to make sure there's no lingering equipment
        provider.Comp.Wearer = null;
        provider.Comp.ConnectedEquipment.Clear();
    }
}
