
using Content.Shared.Item.ItemToggle;
using Content.Shared.Wieldable;
using Content.Shared._DV.Wieldable.Components;

namespace Content.Shared._DV.Wieldable.Systems;

public sealed partial class ToggleItemOnWieldSystem : EntitySystem
{

    [Dependency] private readonly ItemToggleSystem _itemToggle = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ItemToggleOnWieldComponent, ItemWieldedEvent>(OnWield);
        SubscribeLocalEvent<ItemToggleOnWieldComponent, ItemUnwieldedEvent>(OnUnwield);
    }

    private void OnWield(Entity<ItemToggleOnWieldComponent> weapon, ItemWieldedEvent args)
    {
        _itemToggle.TryActivate(weapon, args.User);
    }

    private void OnUnwield(Entity<ItemToggleOnWieldComponent> weapon, ItemUnwieldedEvent args)
    {
        _itemToggle.TryDeactivate(weapon, args.User);
    }
}
