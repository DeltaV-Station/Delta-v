using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.StepTrigger.Components;
using Content.Shared.Tag;

namespace Content.Shared.StepTrigger.Systems;

public sealed class SoftPawsSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SoftPawsComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
    //    SubscribeLocalEvent<SoftPawsComponent, ExaminedEvent>(OnExamined);
    }

    private void OnStepTriggerAttempt(EntityUid uid, SoftPawsComponent component, ref StepTriggerAttemptEvent args)
    {
        if (TryComp<InventoryComponent>(args.Tripper, out var inventory))
        {
            if (!_inventory.TryGetSlotEntity(args.Tripper, "shoes", out _, inventory))
            {
                args.Cancelled = true;
                return;
            }
        }
    }

//    private void OnExamined(EntityUid uid, SoftPawsComponent component, ExaminedEvent args)
//    {
//        args.PushMarkup(Loc.GetString("shoes-required-step-trigger-examine"));
//    }
}
