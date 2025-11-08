using System.Linq;
using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Toggleable;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;

    private void InitializeBipods()
    {
        SubscribeLocalEvent<GunBipodComponent, GotUnequippedHandEvent>(OnUnequip);
        SubscribeLocalEvent<GunBipodComponent, DroppedEvent>(OnDrop);

        SubscribeLocalEvent<GunBipodComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<GunBipodComponent, ToggleActionEvent>(OnToggleAction);

        SubscribeLocalEvent<GunBipodComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<GunBipodComponent, GunRefreshModifiersEvent>(OnGunRefreshModifiers);
        SubscribeLocalEvent<GunBipodComponent, BipodSetupFinishedEvent>(SetupBipod);
        SubscribeLocalEvent<GunBipodComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnMapInit(EntityUid uid, GunBipodComponent component, MapInitEvent args)
    {
        _actionContainer.EnsureAction(uid, ref component.BipodToggleActionEntity, component.BipodToggleAction);
        Dirty(uid, component);
    }

    private void OnUnequip(Entity<GunBipodComponent> ent, ref GotUnequippedHandEvent args)
    {
        PackUpBipod(ent, args.User);
    }

    private void OnDrop(Entity<GunBipodComponent> ent, ref DroppedEvent args)
    {
        PackUpBipod(ent, args.User);
    }

    private void OnGetActions(Entity<GunBipodComponent> ent, ref GetItemActionsEvent args)
    {
        args.AddAction(ref ent.Comp.BipodToggleActionEntity, ent.Comp.BipodToggleAction);
    }

    private void OnToggleAction(Entity<GunBipodComponent> ent, ref ToggleActionEvent args)
    {
        if (args.Handled)
            return;

        var blockQuery = GetEntityQuery<GunBipodComponent>();
        var handQuery = GetEntityQuery<HandsComponent>();

        if (!handQuery.TryGetComponent(args.Performer, out var hands))
            return;

        var bipods = _hands.EnumerateHeld((args.Performer, hands)).ToArray();

        foreach (var bipod in bipods)
        {
            if (bipod == ent.Owner)
                continue;

            if (blockQuery.TryGetComponent(bipod, out var otherBipodComp) && otherBipodComp.IsSetup)
            {
                CantSetupError(args.Performer);
                return;
            }
        }

        if (ent.Comp.IsSetup)
            PackUpBipod(ent, args.Performer);
        else
            TrySetupBipod(ent, args.Performer);

        args.Handled = true;
    }

    private void TrySetupBipod(Entity<GunBipodComponent> ent, EntityUid user)
    {
        if (ent.Comp.IsSetup)
        {
            ent.Comp.BipodSetupTime = TimeSpan.Zero;
            return;
        }

        var xform = Transform(user);

        //Don't allow someone to set up the bipod if they're not parented to a grid
        if (xform.GridUid != xform.ParentUid)
        {
            CantSetupError(user);
            return;
        }

        // Don't allow someone to set up the bipod if they're not holding the weapon
        if (!_hands.IsHolding(user, ent.Owner, out _))
        {
            CantSetupError(user);
            return;
        }

        var gunName = Name(ent.Owner);

        var bipodUser = Identity.Entity(user, EntityManager);
        var msgUser = Loc.GetString("action-popup-bipod-user", ("gunName", gunName));
        var msgOther = Loc.GetString("action-popup-bipod-other", ("bipodUser", bipodUser), ("gunName", gunName));

        Actions.SetToggled(ent.Comp.BipodToggleActionEntity, true);
        PopupSystem.PopupPredicted(msgUser, msgOther, user, user);

        var doAfterArgs = new DoAfterArgs(EntityManager, user, ent.Comp.SetupDelay, new BipodSetupFinishedEvent(), ent.Owner, target: ent.Owner, used: ent.Owner)
        {
            BreakOnDamage = false,
            BreakOnMove = true,
        };

        ent.Comp.BipodSetupTime = Timing.CurTime;
        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnShotAttempted(Entity<GunBipodComponent> ent, ref ShotAttemptedEvent args)
    {
        if (Timing.CurTime < ent.Comp.BipodSetupTime + ent.Comp.SetupDelay)
        {
            args.Cancel();
        }
    }

    private void SetupBipod(Entity<GunBipodComponent> ent, ref BipodSetupFinishedEvent bipodEvent)
    {
        if (bipodEvent.Cancelled)
            return;

        var xform = Transform(bipodEvent.User);

        //Don't allow someone to set up the bipod if they're somehow not anchored.
        TransformSystem.AnchorEntity(bipodEvent.User, xform);
        if (!xform.Anchored)
        {
            CantSetupError(bipodEvent.User);
            return;
        }

        var gunName = Name(ent.Owner);

        var bipodUser = Identity.Entity(bipodEvent.User, EntityManager);
        var msgUser = Loc.GetString("action-popup-bipod-finished-user", ("gunName", gunName));
        var msgOther = Loc.GetString("action-popup-bipod-finished-other", ("bipodUser", bipodUser), ("gunName", gunName));

        Actions.SetToggled(ent.Comp.BipodToggleActionEntity, true);
        PopupSystem.PopupPredicted(msgUser, msgOther, bipodEvent.User, bipodEvent.User);

        ent.Comp.IsSetup = true;

        RefreshModifiers(ent.Owner);
        Dirty(ent);
    }

    private void PackUpBipod(Entity<GunBipodComponent> ent, EntityUid user)
    {
        if (!ent.Comp.IsSetup)
            return;

        var xform = Transform(user);

        var gunName = Name(ent.Owner);

        var bipodUser = Identity.Entity(user, EntityManager);
        var msgUser = Loc.GetString("action-popup-bipod-disabling-user", ("gunName", gunName));
        var msgOther = Loc.GetString("action-popup-bipod-disabling-other", ("bipodUser", bipodUser), ("gunName", gunName));

        if (xform.Anchored)
            TransformSystem.Unanchor(user, xform);

        Actions.SetToggled(ent.Comp.BipodToggleActionEntity, false);
        PopupSystem.PopupPredicted(msgUser, msgOther, user, user);

        ent.Comp.IsSetup = false;

        RefreshModifiers(ent.Owner);
        Dirty(ent);
    }

    private void CantSetupError(EntityUid user)
    {
        var msgError = Loc.GetString("action-popup-bipod-user-cant-setup");
        PopupSystem.PopupClient(msgError, user, user);
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
}

/// <summary>
///     This event gets called when the Setup Doafter is done.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class BipodSetupFinishedEvent : SimpleDoAfterEvent;
