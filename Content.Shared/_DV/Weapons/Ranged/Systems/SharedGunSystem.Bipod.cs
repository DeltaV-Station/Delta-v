using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Toggleable;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Serialization;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainerSystem = default!;

    private void InitializeBipods()
    {
        SubscribeLocalEvent<GunBipodComponent, GotUnequippedHandEvent>(OnUnequip);
        SubscribeLocalEvent<GunBipodComponent, DroppedEvent>(OnDrop);

        SubscribeLocalEvent<GunBipodComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<GunBipodComponent, ToggleActionEvent>(OnToggleAction);

        SubscribeLocalEvent<GunBipodComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<GunBipodComponent, GunRefreshModifiersEvent>(OnGunRefreshModifiers);
        SubscribeLocalEvent<GunBipodComponent, BipodSetupFinishedEvent>(SetupBipod);
        SubscribeLocalEvent<IsUsingBipodComponent, MoveEvent>(OnMove);

        SubscribeLocalEvent<GunBipodComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<IsUsingBipodComponent, ShooterImpulseEvent>(OnImpulse);
    }

    private void OnMapInit(Entity<GunBipodComponent> weapon, ref MapInitEvent args)
    {
        _actionContainerSystem.EnsureAction(weapon.Owner, ref weapon.Comp.BipodToggleActionEntity, weapon.Comp.BipodToggleAction);
        Dirty(weapon);
    }

    private void OnGetActions(Entity<GunBipodComponent> ent, ref GetItemActionsEvent args)
    {
        args.AddAction(ref ent.Comp.BipodToggleActionEntity, ent.Comp.BipodToggleAction);
    }

    private void OnUnequip(Entity<GunBipodComponent> weapon, ref GotUnequippedHandEvent args)
    {
        if (weapon.Comp.IsSetup)
            PackUpBipod(weapon, args.User, null);
    }

    private void OnDrop(Entity<GunBipodComponent> weapon, ref DroppedEvent args)
    {
        if (weapon.Comp.IsSetup)
            PackUpBipod(weapon, args.User, null);
    }

    private void OnMove(Entity<IsUsingBipodComponent> bipodUser, ref MoveEvent args)
    {
        // This fires when the entity rotates. If the position didn't change, do not undo the bipod.
        if (args.OldPosition.Equals(args.NewPosition))
            return;
        // Undo the Bipod of every gun currently used.
        foreach (var weaponUid in bipodUser.Comp.BipodOwnerUids.ToArray())
        {
            if (!TryComp<GunBipodComponent>(weaponUid, out var bipod))
                continue;

            PackUpBipod((weaponUid, bipod), bipodUser.Owner, bipodUser.Comp);
        }
    }

    private void OnToggleAction(Entity<GunBipodComponent> ent, ref ToggleActionEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.IsSetup)
            PackUpBipod(ent, args.Performer, null);
        else
            TrySetupBipod(ent, args.Performer);

        args.Handled = true;
    }

    private void TrySetupBipod(Entity<GunBipodComponent> ent, EntityUid user)
    {
        var xform = Transform(user);

        //Don't allow someone to set up the bipod if they're not parented to a grid
        if (xform.GridUid != xform.ParentUid)
        {
            CantSetupError(user, Loc.GetString("action-popup-bipod-user-cant-setup"));
            return;
        }

        // Don't allow someone to set up the bipod if they're not holding the weapon
        if (!_hands.IsHolding(user, ent.Owner, out _))
        {
            CantSetupError(user, Loc.GetString("action-popup-bipod-user-not-holding"));
            return;
        }

        var gunName = Name(ent.Owner);
        var bipodUser = Identity.Entity(user, EntityManager);
        // Show a popup for everyone to show them setting up their bipod.
        var msgUser = Loc.GetString("action-popup-bipod-user", ("gunName", gunName));
        var msgOther = Loc.GetString("action-popup-bipod-other", ("bipodUser", bipodUser), ("gunName", gunName));

        PopupSystem.PopupPredicted(msgUser, msgOther, user, user);

        var doAfterArgs = new DoAfterArgs(EntityManager, user, ent.Comp.SetupDelay, new BipodSetupFinishedEvent(), ent.Owner, target: ent.Owner, used: ent.Owner)
        {
            BreakOnDamage = false,
            BreakOnMove = true,
        };
        // This is used to prevent the gun from shooting while setting up the bipod.
        ent.Comp.BipodSetupTime = Timing.CurTime;
        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void SetupBipod(Entity<GunBipodComponent> ent, ref BipodSetupFinishedEvent bipodEvent)
    {
        if (bipodEvent.Cancelled) // This method is called whether the DoAfter was successful - Hence the catch.
        {
            ent.Comp.BipodSetupTime = TimeSpan.Zero; // This allows them to shoot again when they cancel putting down the bipod.
            return;
        }

        var bipodUseComp = EnsureComp<IsUsingBipodComponent>(bipodEvent.User); // Set a component on the user so we can track it for movement.
        bipodUseComp.BipodOwnerUids.Add(ent.Owner); // Add it to the used Bipods.

        var gunName = Name(ent.Owner);
        var bipodUser = Identity.Entity(bipodEvent.User, EntityManager);
        // Send another popup that the bipod is successfully set up.
        var msgUser = Loc.GetString("action-popup-bipod-finished-user", ("gunName", gunName));
        var msgOther = Loc.GetString("action-popup-bipod-finished-other", ("bipodUser", bipodUser), ("gunName", gunName));

        Actions.SetToggled(ent.Comp.BipodToggleActionEntity, true);
        PopupSystem.PopupPredicted(msgUser, msgOther, bipodEvent.User, bipodEvent.User);

        ent.Comp.IsSetup = true; // Activate the Bipod.

        RefreshModifiers(ent.Owner); // This will update the modifiers of the weapon, so the bipod is in effect.
        Dirty(ent);
    }

    private void PackUpBipod(Entity<GunBipodComponent> weapon, EntityUid user, IsUsingBipodComponent? userComp)
    {
        if (Timing.ApplyingState
            || !Resolve(user, ref userComp)) // Component can be nullable, so we resolve. Less expensive than TryComp.
            return;

        userComp.BipodOwnerUids.Remove(weapon); // Remove the bipod component from the list of used bipods.
        if (userComp.BipodOwnerUids.Count == 0) // Remove the Component if no bipod is in use anymore.
            RemComp<IsUsingBipodComponent>(user);

        var gunName = Name(weapon);
        var bipodUser = Identity.Entity(user, EntityManager);
        // Show a popup that the bipod has been removed.
        var msgUser = Loc.GetString("action-popup-bipod-disabling-user", ("gunName", gunName));
        var msgOther = Loc.GetString("action-popup-bipod-disabling-other", ("bipodUser", bipodUser), ("gunName", gunName));

        Actions.SetToggled(weapon.Comp.BipodToggleActionEntity, false); // Set the action icon to red.
        PopupSystem.PopupPredicted(msgUser, msgOther, user, user);

        weapon.Comp.IsSetup = false; // Deactivate the Bipod.

        RefreshModifiers(weapon.Owner); // This will update the modifiers of the weapon, so the bipod bonus is lost.
        Dirty(weapon);
    }

    private void CantSetupError(EntityUid user, string errorMessage)
    {
        PopupSystem.PopupClient(errorMessage, user, user);
    }

    private void OnGunRefreshModifiers(Entity<GunBipodComponent> bonus, ref GunRefreshModifiersEvent args)
    {
        if (bonus.Comp.IsSetup)
        {
            args.MinAngle += bonus.Comp.MinAngle;
            args.MaxAngle += bonus.Comp.MaxAngle;
            args.AngleDecay += bonus.Comp.AngleDecay;
            args.AngleIncrease += bonus.Comp.AngleIncrease;
            args.FireRate += bonus.Comp.FireRateIncrease;
        }
    }

    private void OnShotAttempted(Entity<GunBipodComponent> ent, ref ShotAttemptedEvent args)
    {
        // This is true when the bipod is being set up - Preventing the gun from shooting.
        if (Timing.CurTime < ent.Comp.BipodSetupTime + ent.Comp.SetupDelay + TimeSpan.FromSeconds(0.1))
            args.Cancel();
    }

    private void OnImpulse(Entity<IsUsingBipodComponent> bipodUser, ref ShooterImpulseEvent args)
    {
        args.CannotBePushed = true;
    }
}

/// <summary>
///     This event gets called when the Setup Doafter is done.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class BipodSetupFinishedEvent : SimpleDoAfterEvent;
