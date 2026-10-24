using Content.Server.Popups;
using Content.Shared._DV.Clothing.Components;
using Content.Shared.Access.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
namespace Content.Server._DV.Clothing.Systems;
public sealed class JobColorSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedItemSystem _itemSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IdCardComponent, AfterInteractEvent>(OnAfterInteract);
    }
    private void OnAfterInteract(Entity<IdCardComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach)
            return;
        if (!TryComp<JobColorComponent>(args.Target, out var colorSet))
            return;
        if (!colorSet.ManualChange)
            return;
        var jobIcon = ent.Comp.JobIcon;
        var clothing = new Entity<JobColorComponent>(args.Target.Value, colorSet);
        if (!clothing.Comp.JobMap.ContainsKey(jobIcon))
            jobIcon = new("JobIconUnknown");
        if (clothing.Comp.CurrentJobIcon == jobIcon)
        {
            _popupSystem.PopupEntity("The clothing is already set to this job", clothing, args.User);
            return;
        }
        clothing.Comp.CurrentJobIcon = jobIcon;
        Dirty(clothing, clothing.Comp);
        _itemSystem.VisualsChanged(clothing.Owner);
        _popupSystem.PopupEntity("The clothing has matched to the job", clothing, args.User);
        if (TryComp<ToggleableClothingComponent>(clothing.Owner, out var toggleable) &&
        toggleable.ClothingUid != null &&
        TryComp<JobColorComponent>(toggleable.ClothingUid, out var helmetband))
        {
            helmetband.CurrentJobIcon = clothing.Comp.CurrentJobIcon;
            Dirty(toggleable.ClothingUid.Value, helmetband);
            _itemSystem.VisualsChanged(toggleable.ClothingUid.Value);
        }
    }
}
