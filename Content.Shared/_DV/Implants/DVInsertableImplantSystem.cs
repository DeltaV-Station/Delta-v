using Content.Shared.Containers.ItemSlots;
using Content.Shared.Implants.Components;
using Content.Shared.Interaction;

namespace Content.Shared._DV.Implants;

public sealed class DVInsertableImplantSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVInsertableImplantComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<DVInsertableImplantComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || !TryComp<ImplanterComponent>(args.Target, out var implanter))
            return;

        if (_itemSlots.GetItemOrNull(args.Target.Value, ImplanterComponent.ImplanterSlotId) is not null)
            return;

        implanter.ImplantOnly = true;
        Dirty(args.Target.Value, implanter);

        _itemSlots.Insert(args.Target.Value, implanter.ImplanterSlot, ent, args.User, true);
        args.Handled = true;

        _metaData.SetEntityName(args.Target.Value, Loc.GetString(ent.Comp.ImplanterName));
        _metaData.SetEntityDescription(args.Target.Value, Loc.GetString(ent.Comp.ImplanterDescription));
    }
}
