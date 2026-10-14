using System.Diagnostics.CodeAnalysis;
using Content.Shared._DV.MKC;
using Content.Shared.Body;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Shared.Hands.EntitySystems; // DeltaV - cancel doafter on drop

// ReSharper disable InconsistentNaming

namespace Content.Shared._starcup.MKC;

/// <summary>
/// starcup: An electricity-based hunger analogue for MKCs. Players with the power core organ have an internal battery
/// that they need to keep charged to avoid hunger-like effects. They are able to replenish charge by draining anything
/// with BatteryComponent.
/// </summary>
public abstract class SharedPowerCoreSystem : EntitySystem
{
    [Dependency] protected readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedJetpackSystem _jetpack = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!; // DeltaV - cancel doafter on drop

    private readonly SoundSpecifier? _drainSounds = new SoundCollectionSpecifier("sparks");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerCoreComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<PowerDrinkableComponent, GetVerbsEvent<AlternativeVerb>>(OnAlternativeVerbs);
        SubscribeLocalEvent<PowerDrinkableComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<PowerCoreComponent, BodyRelayedEvent<GetVerbsEvent<InnateVerb>>>(AddEnergyDrainVerb);
        SubscribeLocalEvent<PowerCoreComponent, PowerCoreDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<PowerCoreComponent, RefreshChargeRateEvent>(OnRefreshChargeRate);
        SubscribeLocalEvent<PowerCoreComponent, BatteryStateChangedEvent>(OnBatteryStateChanged);
        SubscribeLocalEvent<PowerCoreComponent, BodyRelayedEvent<RefreshMovementSpeedModifiersEvent>>(UpdateMoveSpeedModifier);
        SubscribeLocalEvent<PowerCoreComponent, OrganGotRemovedEvent>(OnOrganRemoved);
        SubscribeLocalEvent<PowerCoreComponent, OrganGotInsertedEvent>(OnOrganInserted);
    }

    private void OnMapInit(Entity<PowerCoreComponent> powerCore, ref MapInitEvent _)
    {
        Entity<BatteryComponent?> battery = (powerCore.Owner, null);
        if (!TryComp(battery, out battery.Comp))
            return;

        _battery.RefreshChargeRate(battery);

        if (!TryGetContainingBody(powerCore, out var body))
            return;

        _movementSpeed.RefreshMovementSpeedModifiers(body.Value);
    }

    private void OnRefreshChargeRate(Entity<PowerCoreComponent> powerCore, ref RefreshChargeRateEvent args)
    {
        // TODO(starcup): This will drain power cores outside of bodies but OrganGotInsertedEvent is raised just
        // before setting the body field on the organ so we can't query it at this point!
        // if (!TryGetContainingBody(powerCore, out var body))
        //     return;
        //
        // if (_mobState.IsDead(body.Value))
        //     return;

        args.NewChargeRate -= powerCore.Comp.WattConsumption;
    }

    private void OnBatteryStateChanged(Entity<PowerCoreComponent> powerCore, ref BatteryStateChangedEvent args)
    {
        if (!TryGetContainingBody(powerCore, out var body))
            return;

        if (args.NewState == BatteryState.Empty || args.OldState == BatteryState.Empty)
            _movementSpeed.RefreshMovementSpeedModifiers(body.Value);
    }

    /// <summary>
    /// Returns the entity, if any, which contains this organ.
    /// </summary>
    /// <param name="powerCore"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    private bool TryGetContainingBody(Entity<PowerCoreComponent> powerCore, [NotNullWhen(true)] out EntityUid? body)
    {
        OrganComponent? organ = null;
        if (!Resolve(powerCore.Owner, ref organ) || organ.Body == null)
        {
            body = null;
            return false;
        }

        body = organ.Body;
        return true;
    }

    /// <summary>
    /// Attempts to find the power core organ inside a mob.
    /// </summary>
    /// <param name="mob">mob to search</param>
    /// <param name="powerCore">power core organ</param>
    /// <returns></returns>
    private bool TryGetPowerCore(EntityUid mob, [NotNullWhen(true)] out Entity<PowerCoreComponent>? powerCore)
    {
        powerCore = null;

        if (!TryComp<BodyComponent>(mob, out var body))
            return false;

        if (body.Organs == null)
            return false;

        foreach (var organ in body.Organs.ContainedEntities)
        {
            if (!TryComp<PowerCoreComponent>(organ, out var comp))
                continue;

            powerCore = (organ, comp);
            return true;
        }

        return false;
    }

    private void UpdateMoveSpeedModifier(Entity<PowerCoreComponent> powerCore, ref BodyRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        Entity<BatteryComponent?> battery = (powerCore.Owner, null);
        if (!Resolve(powerCore.Owner, ref battery.Comp))
            return;

        if (!MathHelper.CloseToPercent(_battery.GetCharge(battery), 0))
            return;

        // No slowdown in weightlessness
        if (_jetpack.IsUserFlying(args.Body.Owner))
            return;

        args.Args.ModifySpeed(powerCore.Comp.LowPowerMovementSpeedMultiplier);
    }

    private void AddEnergyDrainVerb(Entity<PowerCoreComponent> powerCore,
        ref BodyRelayedEvent<GetVerbsEvent<InnateVerb>> args)
    {
        if (!args.Args.CanInteract || !args.Args.CanAccess)
            return;

        if (!HasComp<BatteryComponent>(args.Args.Target))
            return;

        if (HasComp<PowerDrinkableComponent>(args.Args.Target))
            return;

        // DeltaV - drinkable blacklist component
        if (HasComp<PowerDrinkableBlacklistComponent> (args.Args.Target))
            return;

        var target = args.Args.Target;

        InnateVerb verb = new()
        {
            Act = () => TryStartDraining(powerCore, target),
            Text = Loc.GetString("power-core-verb"),
            IconEntity = GetNetEntity(powerCore),
            Priority = 2,
        };
        args.Args.Verbs.Add(verb);
    }

    /// <summary>
    /// Allows MKCs to use alt-E to drain from entities in the world.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="drinkable"></param>
    /// <param name="args"></param>
    private void OnAlternativeVerbs(EntityUid uid, PowerDrinkableComponent drinkable, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!HasComp<BatteryComponent>(args.Target))
            return;

        // DeltaV - drinkable blacklist component
        if (HasComp<PowerDrinkableBlacklistComponent>(args.Target))
            return;

        if (!TryGetPowerCore(args.User, out var powerCore))
            return;

        var verb = new AlternativeVerb
        {
            Act = () => TryStartDraining(powerCore.Value, args.Target),
            Text = Loc.GetString("power-core-verb"),
            IconEntity = GetNetEntity(powerCore),
            Priority = 3,
        };

        args.Verbs.Add(verb);
    }

    /// <summary>
    /// Allows MKCs to hit Z or left-click on a held entity to drain from it.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="args"></param>
    private void OnUseInHand(Entity<PowerDrinkableComponent> entity, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<BatteryComponent>(entity))
            return;

        // DeltaV - drinkable blacklist component
        if (HasComp<PowerDrinkableBlacklistComponent>(entity))
            return;

        if (!TryGetPowerCore(args.User, out var powerCore))
            return;

        args.Handled = TryStartDraining(powerCore.Value, entity);
    }

    private bool TryStartDraining(Entity<PowerCoreComponent> powerCore, EntityUid target)
    {
        if (!TryGetContainingBody(powerCore, out var bodyUid))
            return false;

        Entity<BatteryComponent?> powerCoreBattery = (powerCore.Owner, null);
        if (!Resolve(powerCoreBattery, ref powerCoreBattery.Comp))
            return false;

        Entity<BatteryComponent?> targetBattery = (target, null);
        if (!Resolve(targetBattery, ref targetBattery.Comp))
            return false;

        if (_battery.IsFull(powerCoreBattery))
        {
            _popup.PopupClient(
                Loc.GetString("power-core-full", ("organ", powerCore)),
                bodyUid.Value,
                bodyUid.Value
                );
            return false;
        }

        if (MathHelper.CloseToPercent(_battery.GetCharge(targetBattery), 0) && targetBattery.Comp.NetSyncEnabled)
        {
            _popup.PopupClient(
                Loc.GetString("power-core-battery-empty", ("target", target)),
                bodyUid.Value,
                bodyUid.Value
                );
            return false;
        }

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, bodyUid.Value, powerCore.Comp.Delay, new PowerCoreDoAfterEvent(), powerCore, target: target, used: powerCore)
        {
            DuplicateCondition = DuplicateConditions.SameEvent,
            BreakOnMove = false,
            BreakOnHandChange = false,
            BreakOnDamage = true,
            NeedHand = _hands.IsHolding(bodyUid.Value, target), // DeltaV - stop power drink on dropping cell
        });
        return true;
    }

    private void OnDoAfter(Entity<PowerCoreComponent> powerCore, ref PowerCoreDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target is not {} target)
            return;

        Entity<BatteryComponent?> powerCoreBattery = (powerCore.Owner, null);
        if (!Resolve(powerCoreBattery, ref powerCoreBattery.Comp))
            return;

        Entity<BatteryComponent?> targetBattery = (target, null);
        if (!Resolve(targetBattery, ref targetBattery.Comp))
            return;

        Drink(powerCore, targetBattery);

        var powerCoreBatteryIsFull = _battery.IsFull(powerCoreBattery);
        var targetBatteryIsEmpty = MathHelper.CloseToPercent(_battery.GetCharge(targetBattery), 0);

        args.Repeat = !powerCoreBatteryIsFull && !targetBatteryIsEmpty;
    }

    private void Drink(Entity<PowerCoreComponent> powerCore, Entity<BatteryComponent?> target)
    {
        if (!TryGetContainingBody(powerCore, out var body))
            return;

        var powerCoreBattery = new Entity<BatteryComponent?>(powerCore.Owner, null);
        if (!Resolve(powerCore.Owner, ref powerCoreBattery.Comp))
            return;

        var powerCoreBatteryCharge = _battery.GetCharge(powerCoreBattery);
        var targetBatteryCharge = _battery.GetCharge(target);

        var joulesNeeded = Math.Max(powerCoreBattery.Comp.MaxCharge - powerCoreBatteryCharge, 0);
        var joulesToDrain = Math.Min(targetBatteryCharge, joulesNeeded);
        joulesToDrain = Math.Min(joulesToDrain, powerCore.Comp.JoulesPerDrain);

        if (joulesToDrain <= 0f)
            return;

        _battery.SetCharge(powerCoreBattery, powerCoreBatteryCharge + joulesToDrain);
        _battery.SetCharge(target, targetBatteryCharge - joulesToDrain);

        _audio.PlayPredicted(_drainSounds, target.Owner, body);
        if (_timing.IsFirstTimePredicted)
            Spawn("EffectSparks", Transform(target.Owner).Coordinates);

        _popup.PopupPredicted(
            Loc.GetString("power-core-drain", ("target", target)),
            powerCore.Owner,
            powerCore.Owner
            );
    }

    private void OnOrganRemoved(Entity<PowerCoreComponent> powerCore, ref OrganGotRemovedEvent args)
    {
        _battery.RefreshChargeRate(powerCore.Owner);
        _movementSpeed.RefreshMovementSpeedModifiers(args.Target);
    }

    private void OnOrganInserted(Entity<PowerCoreComponent> powerCore, ref OrganGotInsertedEvent args)
    {
        _battery.RefreshChargeRate(powerCore.Owner);
        _movementSpeed.RefreshMovementSpeedModifiers(args.Target);
    }
}
