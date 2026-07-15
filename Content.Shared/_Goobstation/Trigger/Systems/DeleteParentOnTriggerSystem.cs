using Content.Shared.Inventory;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Systems;
using Content.Shared.Trigger.Components.Effects;
using Content.Shared._Goobstation.Trigger.Components.Effects;
using Content.Shared.Body;

namespace Content.Shared._Goobstation.Trigger.Systems;

public sealed class DeleteParentOnTriggerSystem : EntitySystem
{
    // [Dependency] private readonly BodySystem _body = default!; // Delta V - Not used
    // [Dependency] private readonly InventorySystem _inventory = default!; // Delta V - Not used

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeleteParentOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(Entity<DeleteParentOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;

        var target = ent.Comp.TargetUser ? args.User : ent.Owner;

        if (!TryComp<TransformComponent>(target, out var userXform)) // Delta V - was ent.Owner before. Changed to unused target
            return;

        if (userXform.ParentUid == userXform.GridUid || userXform.ParentUid == userXform.MapUid)
            return;

        PredictedQueueDel(userXform.ParentUid);
        args.Handled = true;
    }
}
