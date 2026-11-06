using Content.Shared.Actions.Events;

namespace Content.Shared._DV.Actions;

public sealed class DeleteOnPerformedActionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeleteOnPerformedActionComponent, ActionPerformedEvent>(OnActionPerformed);
    }

    private void OnActionPerformed(Entity<DeleteOnPerformedActionComponent> ent, ref ActionPerformedEvent args)
    {
        PredictedQueueDel(ent);
    }
}
