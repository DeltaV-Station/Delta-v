using System.Linq;
using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Actions;
using Content.Shared.Blocking;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Toggleable;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly FixtureSystem _fixtureSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

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
        if (bipodEvent.Cancelled || ent.Comp.IsSetup)
            return;

        var user = bipodEvent.User;

        var xform = Transform(user);

        var bipod = Name(ent.Owner);

        var bipodUser = Identity.Entity(user, EntityManager);
        var msgUser = Loc.GetString("action-popup-bipod-user", ("bipodName", bipod));
        var msgOther = Loc.GetString("action-popup-bipod-other", ("bipodUser", bipodUser), ("bipodName", bipod));

        //Don't allow someone to set up the bipod if they're not parented to a grid
        if (xform.GridUid != xform.ParentUid)
        {
            CantSetupError(user);
            return;
        }

        // Don't allow someone to set up the bipod if they're not holding the shield
        if (!_hands.IsHolding(user, ent.Owner, out _))
        {
            CantSetupError(user);
            return;
        }

        //Don't allow someone to set up the bipod if they're somehow not anchored.
        TransformSystem.AnchorEntity(user, xform);
        if (!xform.Anchored)
        {
            CantSetupError(user);
            return;
        }
        Actions.SetToggled(ent.Comp.BipodToggleActionEntity, true);
        PopupSystem.PopupPredicted(msgUser, msgOther, user, user);

        ent.Comp.IsSetup = true;

        RefreshModifiers(ent.Owner);
        Dirty(ent);
    }

    private void PackUpBipod(Entity<GunBipodComponent> ent, EntityUid user)
    {
        if (!ent.Comp.IsSetup)
            return;

        var xform = Transform(user);

        var bipodName = Name(ent.Owner);

        var bipodUser = Identity.Entity(user, EntityManager);
        var msgUser = Loc.GetString("action-popup-bipod-disabling-user", ("bipod", bipodName));
        var msgOther = Loc.GetString("action-popup-bipod-disabling-other", ("bipodUser", bipodUser), ("bipod", bipodName));

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
        var msgError = Loc.GetString("action-popup-blocking-user-cant-block");
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
