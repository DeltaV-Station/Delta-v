using Content.Shared.Actions;
using Content.Shared.Climbing.Components;
using Content.Shared.Climbing.Events;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._DV.Abilities;

/// <summary>
/// Not to be confused with laying down, <see cref="CrawlUnderObjectsComponent"/> lets you move under tables.
/// </summary>
public sealed class CrawlUnderObjectsSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _moveSpeed = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrawlUnderObjectsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CrawlUnderObjectsComponent, ToggleCrawlingStateEvent>(OnToggleCrawling);
        SubscribeLocalEvent<CrawlUnderObjectsComponent, AttemptClimbEvent>(OnAttemptClimb);
        SubscribeLocalEvent<CrawlUnderObjectsComponent, DownAttemptEvent>(CancelWhenSneaking);
        SubscribeLocalEvent<CrawlUnderObjectsComponent, StandAttemptEvent>(CancelWhenSneaking);
        SubscribeLocalEvent<CrawlUnderObjectsComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<CrawlUnderObjectsComponent, MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<FixturesComponent, CrawlingUpdatedEvent>(OnCrawlingUpdated);
    }

    private void OnMapInit(Entity<CrawlUnderObjectsComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.ToggleHideAction != null)
            return;

        _actions.AddAction(ent, ref ent.Comp.ToggleHideAction, ent.Comp.ActionProto);
    }

    private void OnToggleCrawling(Entity<CrawlUnderObjectsComponent> ent, ref ToggleCrawlingStateEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryToggle(ent);
    }

    private void OnAttemptClimb(Entity<CrawlUnderObjectsComponent> ent, ref AttemptClimbEvent args)
    {
        if (ent.Comp.Enabled)
            args.Cancelled = true;
    }

    private void CancelWhenSneaking<TEvent>(Entity<CrawlUnderObjectsComponent> ent, ref TEvent args) where TEvent : CancellableEntityEventArgs
    {
        if (ent.Comp.Enabled)
            args.Cancel();
    }

    private void OnRefreshMoveSpeed(Entity<CrawlUnderObjectsComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Enabled)
            args.ModifySpeed(ent.Comp.SneakSpeedModifier, ent.Comp.SneakSpeedModifier);
    }

    private void OnMobStateChanged(Entity<CrawlUnderObjectsComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.OldMobState != MobState.Alive || !ent.Comp.Enabled)
            return;

        // crawling prevents downing, so when you go crit/die stop crawling and force downing
        SetEnabled(ent, false);
        _standing.Down(ent);
    }

    private void OnCrawlingUpdated(Entity<FixturesComponent> ent, ref CrawlingUpdatedEvent args)
    {
        if (args.Enabled)
        {
            foreach (var (key, fixture) in ent.Comp.Fixtures)
            {
                var newMask = (fixture.CollisionMask
                    & (int)~CollisionGroup.HighImpassable
                    & (int)~CollisionGroup.MidImpassable)
                    | (int)CollisionGroup.InteractImpassable;
                if (fixture.CollisionMask == newMask)
                    continue;

                args.Comp.ChangedFixtures.Add((key, fixture.CollisionMask));
                _physics.SetCollisionMask(ent,
                    key,
                    fixture,
                    newMask,
                    manager: ent.Comp);
            }
        }
        else
        {
            foreach (var (key, originalMask) in args.Comp.ChangedFixtures)
            {
                if (ent.Comp.Fixtures.TryGetValue(key, out var fixture))
                    _physics.SetCollisionMask(ent, key, fixture, originalMask, ent.Comp);
            }

            args.Comp.ChangedFixtures.Clear();
        }
    }

    /// <summary>
    /// Tries to enable or disable sneaking
    /// </summary>
    public bool TrySetEnabled(Entity<CrawlUnderObjectsComponent> ent, bool enabled)
    {
        if (ent.Comp.Enabled == enabled || IsOnCollidingTile(ent) || _standing.IsDown(ent))
            return false;

        if (TryComp<ClimbingComponent>(ent, out var climbing) && climbing.IsClimbing)
            return false;

        SetEnabled(ent, enabled);

        var msg = Loc.GetString("crawl-under-objects-toggle-" + (enabled ? "on" : "off"));
        _popup.PopupPredicted(msg, ent, ent);

        return true;
    }

    private void SetEnabled(Entity<CrawlUnderObjectsComponent> ent, bool enabled)
    {
        ent.Comp.Enabled = enabled;
        Dirty(ent);

        _appearance.SetData(ent, SneakingVisuals.Sneaking, enabled);

        _moveSpeed.RefreshMovementSpeedModifiers(ent);

        var ev = new CrawlingUpdatedEvent(enabled, ent.Comp);
        RaiseLocalEvent(ent, ref ev);
    }

    /// <summary>
    /// Tries to toggle sneaking
    /// </summary>
    public bool TryToggle(Entity<CrawlUnderObjectsComponent> ent)
    {
        return TrySetEnabled(ent, !ent.Comp.Enabled);
    }

    private bool IsOnCollidingTile(EntityUid uid)
    {

        var coords = Transform(uid).Coordinates;

        if (_turf.GetTileRef(coords) is not { } tile)
            return false;

        return _turf.IsTileBlocked(tile, CollisionGroup.MobMask);
    }
}
