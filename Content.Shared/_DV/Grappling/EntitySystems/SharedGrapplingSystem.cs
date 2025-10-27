using Content.Shared._DV.Grappling.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Events;
using Content.Shared.Standing;

namespace Content.Shared._DV.Grappling.EntitySystems;

/// <summary>
/// Shared logic for grapplers.
/// Enables some prediction of events and updates.
/// </summary>
public abstract partial class SharedGrapplingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrapplerComponent, UpdateCanMoveEvent>(OnCanMoveQuery);
        SubscribeLocalEvent<GrapplerComponent, AttackAttemptEvent>(OnAttemptAttack);

        SubscribeLocalEvent<GrappledComponent, StandAttemptEvent>(OnGrappledStand);
        SubscribeLocalEvent<GrappledComponent, UpdateCanMoveEvent>(OnGrappleCanMoveQuery);
    }

    /// <summary>
    /// Handles when a grappler attempts to move.
    /// Potentially disallows movement of the grappler when they are grappling a target.
    /// </summary>
    /// <param name="grappler">The grappling entity.</param>
    /// <param name="args">Args for the event.</param>
    private void OnCanMoveQuery(Entity<GrapplerComponent> grappler, ref UpdateCanMoveEvent args)
    {
        if (grappler.Comp.CanMoveWhileGrappling)
            return; // This entity can always move.

        if (grappler.Comp.ActiveVictim.HasValue)
            args.Cancel(); // Can't move while grappling
    }

    /// <summary>
    /// Handles when a grappled target attempts to stand and blocks it.
    /// </summary>
    /// <param name="grappled">Grappled entity attempting to stand.</param>
    /// <param name="args">Args for the event.</param>
    private void OnGrappledStand(Entity<GrappledComponent> grappled, ref StandAttemptEvent args)
    {
        args.Cancel(); // Can't stand while being grappled
    }

    /// <summary>
    /// Handles when a grappled target attempts to move and blocks it.
    /// </summary>
    /// <param name="grappled">Grappled entity attempting to move.</param>
    /// <param name="args">Args for the event.</param>
    private void OnGrappleCanMoveQuery(Entity<GrappledComponent> grappled, ref UpdateCanMoveEvent args)
    {
        args.Cancel(); // Can't move while grappled
    }

    /// <summary>
    /// Handles when a grappler attempts to attack an entity.
    /// If they have an active victim, they will not be able to attack because their body
    /// is currently being used in the grapple.
    /// </summary>
    /// <param name="grappler">Grappler attempting to attack an entity.</param>
    /// <param name="args">Args for the event.</param>
    private void OnAttemptAttack(Entity<GrapplerComponent> grappler, ref AttackAttemptEvent args)
    {
        if (grappler.Comp.ActiveVictim.HasValue)
            args.Cancel(); // You cannot attack while grappling
    }

}
