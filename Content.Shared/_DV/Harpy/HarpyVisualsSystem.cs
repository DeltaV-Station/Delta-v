using Content.Shared.Body;
using Content.Shared.Inventory.Events;
using Content.Shared.Tag;
using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Harpy;

public sealed class HarpyVisualsSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;

    private readonly ProtoId<TagPrototype> HarpyWingsTag = "HidesHarpyWings";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HarpySingerComponent, DidEquipEvent>(OnDidEquipEvent);
        SubscribeLocalEvent<HarpySingerComponent, DidUnequipEvent>(OnDidUnequipEvent);
    }

    private void OnDidEquipEvent(EntityUid uid, HarpySingerComponent component, DidEquipEvent args)
    {
        if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, HarpyWingsTag))
        {
            // Delta V - Begin make this system work with Nubody
            var evWing = new HumanoidLayerVisibilityChangedEvent(HumanoidVisualLayers.RArmExtension, false);
            var evTail = new HumanoidLayerVisibilityChangedEvent(HumanoidVisualLayers.Tail, false);

            RaiseLocalEvent(uid, ref evWing);
            RaiseLocalEvent(uid, ref evTail);
            // Delta V - End
        }
    }

    private void OnDidUnequipEvent(EntityUid uid, HarpySingerComponent component, DidUnequipEvent args)
    {
        if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, HarpyWingsTag))
        {
            // Delta V - Begin make this system work with Nubody
            var evWing = new HumanoidLayerVisibilityChangedEvent(HumanoidVisualLayers.RArmExtension, true);
            var evTail = new HumanoidLayerVisibilityChangedEvent(HumanoidVisualLayers.Tail, true);

            RaiseLocalEvent(uid, ref evWing);
            RaiseLocalEvent(uid, ref evTail);
            // Delta V - End
        }
    }
}
