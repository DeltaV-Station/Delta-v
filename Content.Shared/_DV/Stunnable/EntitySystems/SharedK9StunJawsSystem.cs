using Content.Shared._DV.Stunnable.Components;
using Content.Shared._DV.Stunnable.Events;
using Content.Shared.Actions;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Stunnable.EntitySystems;

public abstract partial class SharedK9StunJawsSystem : EntitySystem
{
    [Dependency] protected readonly SharedActionsSystem ActionSystem = default!;
    private readonly EntProtoId _toggleAction = "ActionToggleK9ShockJaws";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<K9StunJawsComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<K9StunJawsComponent, ToggleK9ShockJawsEvent>(OnJawsToggled);
    }

    /// <summary>
    /// Handles when the component starts on an entity.
    /// </summary>
    /// <param name="ent">The jaws component that has been added to the entity.</param>
    /// <param name="args">Args for the event.</param>
    protected virtual void OnComponentStartup(Entity<K9StunJawsComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.ActionEntity = ActionSystem.AddAction(ent.Owner, _toggleAction);
    }

    /// <summary>
    /// Handles when the user activates the toggle action for the jaws, setting them to on or off.
    /// </summary>
    /// <param name="ent">Entity which has toggled the action.</param>
    /// <param name="args">Args for the event.</param>
    protected virtual void OnJawsToggled(Entity<K9StunJawsComponent> ent, ref ToggleK9ShockJawsEvent args)
    {
        ent.Comp.Active = !ent.Comp.Active;
        ActionSystem.SetToggled(ent.Comp.ActionEntity, ent.Comp.Active);
        ToggleJawsDamage(ent, ent.Comp.Active);
    }

    /// <summary>
    /// Attempts to toggle the damage for the K9 jaws that comes from an ItemToggleMeleeWeaponComponent.
    /// Using an actual ItemToggle component, and the systme, would mean the holder of this component can be
    /// toggled like a stun baton, which is not what we want.
    /// </summary>
    /// <param name="ent">Entity to toggle item damage for</param>
    /// <param name="active">Whether to toggle the active or inactive item damage set.</param>
    protected void ToggleJawsDamage(Entity<K9StunJawsComponent> ent, bool active)
    {
        var ev = new ItemToggledEvent(true, active, ent.Owner);
        RaiseLocalEvent(ent.Owner, ref ev);
    }
}
