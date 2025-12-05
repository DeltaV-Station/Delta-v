using Content.Shared.Actions;
using Content.Shared.Movement.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared._EE.Flight.Events;
using Content.Shared.Standing;

namespace Content.Shared._EE.Flight;
public abstract class SharedFlightSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private readonly SharedStaminaSystem _staminaSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!; // DeltaV

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlightComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FlightComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FlightComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<FlightComponent, RefreshFrictionModifiersEvent>(OnRefreshFrictionModifiers);
    }

    #region Core Functions
    private void OnStartup(EntityUid uid, FlightComponent component, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
    }

    private void OnShutdown(EntityUid uid, FlightComponent component, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, component.ToggleActionEntity);
    }

    public void ToggleActive(Entity<FlightComponent> ent, bool active)
    {
        // TODO: Add standing check and ftl string if not standing
        if(TryComp<StandingStateComponent>(ent, out var standing) && _standing.IsDown((ent, standing)))
            return; // TODO: pop-up?

        ent.Comp.IsCurrentlyFlying = active;
        ent.Comp.TimeUntilFlap = 0f;
        _actionsSystem.SetToggled(ent.Comp.ToggleActionEntity, ent.Comp.IsCurrentlyFlying);
        RaiseNetworkEvent(new FlightEvent(GetNetEntity(ent), ent.Comp.IsCurrentlyFlying, ent.Comp.IsAnimated));
        _staminaSystem.ToggleStaminaDrain(ent, ent.Comp.StaminaDrainRate, active, false);
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
        _movementSpeed.RefreshFrictionModifiers(ent);
        UpdateHands(ent, active);
        Dirty(ent, ent.Comp);
    }

    private void UpdateHands(EntityUid uid, bool flying)
    {
        if (!TryComp<HandsComponent>(uid, out var handsComponent))
            return;

        if (flying)
            BlockHands(uid, handsComponent);
        else
            FreeHands(uid);
    }

    private void BlockHands(EntityUid uid, HandsComponent handsComponent)
    {
        var freeHands = 0;
        foreach (var hand in _hands.EnumerateHands((uid, handsComponent)))
        {
            if (_hands.TryGetHeldItem((uid, handsComponent), hand, out var heldItem))
            {
                freeHands++;
                continue;
            }

            // Is this entity removable? (they might have handcuffs on)
            if (HasComp<UnremoveableComponent>(heldItem) && heldItem != uid)
                continue;

            _hands.DoDrop((uid, handsComponent), hand, true);
            freeHands++;
            if (freeHands == 2)
                break;
        }
        if (_virtualItem.TrySpawnVirtualItemInHand(uid, uid, out var virtItem1))
            EnsureComp<UnremoveableComponent>(virtItem1.Value);

        if (_virtualItem.TrySpawnVirtualItemInHand(uid, uid, out var virtItem2))
            EnsureComp<UnremoveableComponent>(virtItem2.Value);
    }

    private void FreeHands(EntityUid uid)
    {
        _virtualItem.DeleteInHandsMatching(uid, uid);
    }

    private void OnRefreshMoveSpeed(EntityUid uid, FlightComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (!component.IsCurrentlyFlying)
            return;

        args.ModifySpeed(component.SpeedModifier, component.SpeedModifier);
    }

    #endregion

    private void OnRefreshFrictionModifiers(Entity<FlightComponent> ent, ref RefreshFrictionModifiersEvent args)
    {
        if (!ent.Comp.IsCurrentlyFlying)
            return;

        args.ModifyFriction(ent.Comp.FrictionModifier, ent.Comp.FrictionModifier);
        args.ModifyAcceleration(ent.Comp.AccelerationModifer);
    }
}
public sealed partial class ToggleFlightEvent : InstantActionEvent { }