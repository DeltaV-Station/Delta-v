using Content.Shared._DV.Stunnable.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Organ;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Stunnable.EntitySystems;

public abstract class SharedK9ShockJawsSystem : EntitySystem
{
    private readonly EntProtoId _toggleAction = "ActionToggleK9ShockJaws";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<K9ShockJawsComponent, GetItemActionsEvent>(OnGetActions);
    }

    /// <summary>
    /// Handles when the Shock Jaws augment is queried for additional actions to grant.
    /// </summary>
    /// <param name="ent">Entity being added as an orgna.</param>
    /// <param name="args">Args for the event.</param>
    private void OnGetActions(Entity<K9ShockJawsComponent> ent, ref GetItemActionsEvent args)
    {
        if (!TryComp<OrganComponent>(ent, out var organ) || organ.Body is not { } body)
            return; // This needs to be attached to a body in order to function

        args.AddAction(ref ent.Comp.ActionEntity, _toggleAction);
    }
}
