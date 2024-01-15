using Content.Shared.Clothing.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.DeltaV.Harpy;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Server.GameObjects;

namespace Content.Server.DeltaV.Harpy;

public abstract class HarpyVisualsSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;

    [ValidatePrototypeId<TagPrototype>]
    private const string HarpyWingsTag = "HidesHarpyWings";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HarpyVisualsComponent, DidEquipEvent>(OnHardsuitEquip);
        SubscribeLocalEvent<HarpyVisualsComponent, DidUnequipEvent>(OnHardsuitUnequip);
    }

    private void OnHardsuitEquip(EntityUid uid, HarpyVisualsComponent component, DidEquipEvent args)
    {
        if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, HarpyWingsTag))
        {
            _appearanceSystem.SetData(uid, HardsuitWings.Worn, true);
        }
    }

    private void OnHardsuitUnequip(EntityUid uid, HarpyVisualsComponent component, DidUnequipEvent args)
    {
        if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, HarpyWingsTag))
        {
            _appearanceSystem.SetData(uid, HardsuitWings.Worn, false);
        }
    }
}
