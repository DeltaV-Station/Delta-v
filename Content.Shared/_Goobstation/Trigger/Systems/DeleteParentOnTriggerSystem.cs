using Content.Shared.Body.Systems;
using Content.Shared.Inventory;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Systems;
using Content.Shared.Trigger.Components.Effects;
using Content.Shared._Goobstation.Trigger.Components.Effects;

namespace Content.Shared._Goobstation.Trigger.Systems;

public sealed class DeleteParentOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

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

        if (!TryComp<TransformComponent>(ent.Owner, out var userXform))
            return;

        if (userXform.ParentUid == userXform.GridUid || userXform.ParentUid == userXform.MapUid)
            return;

        EntityManager.QueueDeleteEntity(userXform.ParentUid);
        args.Handled = true;
    }
}
