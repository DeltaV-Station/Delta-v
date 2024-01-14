//using Content.Shared.Examine;
//using Content.Shared.Inventory;
//using Content.Shared.StepTrigger.Components;
//using Content.Shared.Tag;

//namespace Content.Shared.StepTrigger.Systems;

//public sealed class SoftPawsSystem : EntitySystem
//{
   // [Dependency] private readonly InventorySystem _inventory = default!;
   // [Dependency] private readonly TagSystem _tagSystem = default!;

    // <inheritdoc/> 
   // public override void Initialize()
   // {
   //     SubscribeLocalEvent<ShoesRequiredStepTriggerComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
   //     SubscribeLocalEvent<ShoesRequiredStepTriggerComponent, ExaminedEvent>(OnExamined);
    //}

    //private void OnStepTriggerAttempt(EntityUid uid, ShoesRequiredStepTriggerComponent component, ref StepTriggerAttemptEvent args)
    //{
    //    if (TryComp<InventoryComponent>(args.Tripper, out var inventory))
    //    {
    //        if (_tagSystem.HasTag(args.Tripper, "SoftPaws") & !_inventory.TryGetSlotEntity(args.Tripper, "shoes", out _, inventory))
    //        {
    //            args.Cancelled = true;
     //           return;
      //      }
      //  }
   // }

   // private void OnExamined(EntityUid uid, ShoesRequiredStepTriggerComponent component, ExaminedEvent args)
   // {
   //     args.PushMarkup(Loc.GetString("shoes-required-step-trigger-examine"));
   // }
//}
