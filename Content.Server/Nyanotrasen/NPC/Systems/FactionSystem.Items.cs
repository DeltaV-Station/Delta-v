using Content.Server.NPC.Components;
using Content.Server.Store.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;

namespace Content.Server.NPC.Systems;

public partial class NpcFactionSystem : EntitySystem
{
    public void InitializeItems()
    {
        SubscribeLocalEvent<NpcFactionMemberComponent, ItemPurchasedEvent>(OnItemPurchased);

        SubscribeLocalEvent<ClothingAddFactionComponent, GotEquippedEvent>(OnClothingEquipped);
        SubscribeLocalEvent<ClothingAddFactionComponent, GotUnequippedEvent>(OnClothingUnequipped);
    }

    /// <summary>
    /// If we bought something we probably don't want it to start biting us after it's automatically placed in our hands.
    /// If you do, consider finding a better solution to grenade penguin CBT.
    /// </summary>
    private void OnItemPurchased(EntityUid uid, NpcFactionMemberComponent component, ref ItemPurchasedEvent args)
    {
        component.ExceptionalFriendlies.Add(args.Purchaser);
    }

    private void OnClothingEquipped(EntityUid uid, ClothingAddFactionComponent component, GotEquippedEvent args)
    {
        if (!TryComp<ClothingComponent>(uid, out var clothing))
            return;

        if (!clothing.Slots.HasFlag(args.SlotFlags))
            return;

        if (!TryComp<NpcFactionMemberComponent>(args.Equipee, out var factionComponent))
            return;

        if (factionComponent.Factions.Contains(component.Faction))
            return;

        component.IsActive = true;
        AddFaction(args.Equipee, component.Faction);
    }

    private void OnClothingUnequipped(EntityUid uid, ClothingAddFactionComponent component, GotUnequippedEvent args)
    {
        if (!component.IsActive)
            return;

        component.IsActive = false;
        RemoveFaction(args.Equipee, component.Faction);
    }
}
