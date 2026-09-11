using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Tag;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared._Goobstation.Clothing.Systems;

public sealed class ClothingGrantingSystem : EntitySystem
{
    [Dependency] private IComponentFactory _componentFactory = default!;
    [Dependency] private ISerializationManager _serializationManager = default!;
    [Dependency] private TagSystem _tagSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Content.Shared._Goobstation.Clothing.Components.ClothingGrantComponentComponent, GotEquippedEvent>(OnCompEquip);
        SubscribeLocalEvent<Content.Shared._Goobstation.Clothing.Components.ClothingGrantComponentComponent, GotUnequippedEvent>(OnCompUnequip);

        SubscribeLocalEvent<Content.Shared._Goobstation.Clothing.Components.ClothingGrantTagComponent, GotEquippedEvent>(OnTagEquip);
        SubscribeLocalEvent<Content.Shared._Goobstation.Clothing.Components.ClothingGrantTagComponent, GotUnequippedEvent>(OnTagUnequip);
    }

    private void OnCompEquip(EntityUid uid, Content.Shared._Goobstation.Clothing.Components.ClothingGrantComponentComponent component, GotEquippedEvent args)
    {
        if (!TryComp<ClothingComponent>(uid, out var clothing)) return;

        if (!clothing.Slots.HasFlag(args.SlotFlags)) return;

        // Goobstation
        //if (component.Components.Count > 1)
        //{
        //    Logger.Error("Although a component registry supports multiple components, we cannot bookkeep more than 1 component for ClothingGrantComponent at this time.");
        //    return;
        //}

        foreach (var (name, data) in component.Components)
        {
            var newComp = (Component) _componentFactory.GetComponent(name);

            if (HasComp(args.EquipTarget, newComp.GetType()))
                continue;

            newComp.Owner = args.EquipTarget;

            var temp = (object) newComp;
            _serializationManager.CopyTo(data.Component, ref temp);
            AddComp(args.EquipTarget, (Component)temp!);

            component.Active[name] = true; // Goobstation
        }
    }

    private void OnCompUnequip(EntityUid uid, Content.Shared._Goobstation.Clothing.Components.ClothingGrantComponentComponent component, GotUnequippedEvent args)
    {
        // Goobstation
        //if (!component.IsActive) return;

        foreach (var (name, data) in component.Components)
        {
            // Goobstation
            if (!component.Active.ContainsKey(name) || !component.Active[name])
                continue;

            var newComp = (Component) _componentFactory.GetComponent(name);

            RemComp(args.EquipTarget, newComp.GetType());
            component.Active[name] = false; // Goobstation
        }

        // Goobstation
        //component.IsActive = false;
    }


    private void OnTagEquip(EntityUid uid, Content.Shared._Goobstation.Clothing.Components.ClothingGrantTagComponent component, GotEquippedEvent args)
    {
        if (!TryComp<ClothingComponent>(uid, out var clothing))
            return;

        if (!clothing.Slots.HasFlag(args.SlotFlags))
            return;

        EnsureComp<TagComponent>(args.EquipTarget);
        _tagSystem.AddTag(args.EquipTarget, component.Tag);

        component.IsActive = true;
    }

    private void OnTagUnequip(EntityUid uid, Content.Shared._Goobstation.Clothing.Components.ClothingGrantTagComponent component, GotUnequippedEvent args)
    {
        if (!component.IsActive)
            return;

        _tagSystem.RemoveTag(args.EquipTarget, component.Tag);

        component.IsActive = false;
    }
}
