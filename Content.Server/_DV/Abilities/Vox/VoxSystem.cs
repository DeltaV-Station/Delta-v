using Content.Shared._DV.Abilities;
using Content.Shared.Actions;
using Content.Shared.Body.Organ;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Materials;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Abilities.Vox;

public sealed class VoxSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ItemCougherSystem _cougher = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VoxComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<VoxComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, VoxComponent comp, AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach || args.Handled)
            return;

        var target = args.Target.Value;

        if (!TryComp<OrganComponent>(uid, out var organ) || organ.Body is not { } body)
            return;

        if (!TryComp<ItemComponent>(target, out var itemComp) ||
            _item.GetSizePrototype(itemComp.Size).Weight > _prototype.Index(comp.MaxSwallowSize).Weight)
        {
            _popup.PopupEntity("Too large to swallow!", body, body);
            return;
        }

        if (!TryComp<PhysicalCompositionComponent>(target, out var composition) ||
            composition.MaterialComposition.Count == 0)
            return;

        // --- DYNAMIC DIGESTION LOGIC ---
        float nutriBurnRate = 0f;
        if (TryComp<HungerComponent>(body, out var hunger))
        {
            // Calculate how "full" the Vox is (0.0 to 1.0+)
            var hungerRatio = HungerSystem.GetHunger(hunger) / HungerComponent.Thresholds[HungerThreshold.Okay];

            // If we are below the threshold, calculate additional burn for nutrition
            if (hungerRatio < comp.NutritionThreshold)
            {
                // Scaling: The hungrier you are, the more you burn.
                // Max additional burn is 30% when hunger is 0.
                nutriBurnRate = (comp.NutritionThreshold - hungerRatio) * 0.375f;
            }
        }

        foreach (var (matId, amount) in composition.MaterialComposition)
        {
            // Logic partitioning:
            // 1. Waste (BaseWasteRate) is always gone.
            // 2. NutriBurn is taken if hungry.
            // 3. Remainder is stored.

            float nutritionValue = amount * nutriBurnRate;
            float wastedValue = amount * comp.BaseWasteRate;
            float storedValue = amount - nutritionValue - wastedValue;

            if (nutritionValue > 0)
                _hunger.ModifyHunger(body, nutritionValue / 10f);

            comp.StoredMatter[matId] = comp.StoredMatter.GetValueOrDefault(matId) + MathF.Max(0, storedValue);
        }

        _popup.PopupEntity("You swallow the item and feel your gizzard grinding.", body, body);
        QueueDel(target);
        CheckActionAvailability(body, comp);
        args.Handled = true;
    }

    private void CheckActionAvailability(EntityUid body, VoxComponent comp)
    {
        bool hasEnough = false;
        foreach (var amount in comp.StoredMatter.Values)
        {
            if (amount >= comp.MaterialUnitPerSheet)
            {
                hasEnough = true;
                break;
            }
        }

        if (hasEnough && comp.CoughActionEntity == null)
            _actions.AddAction(body, ref comp.CoughActionEntity, "ActionVoxRegurgitate");
        else if (!hasEnough && comp.CoughActionEntity != null)
        {
            _actions.RemoveAction(body, comp.CoughActionEntity);
            comp.CoughActionEntity = null;
        }
    }


    private void OnMapInit(Entity<VoxComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.UpdateInterval;
    }

}
