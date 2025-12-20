using Content.Shared.Damage.Events;

namespace Content.Shared._DV.Augments;

/// <summary>
/// Handling for stamina related event forwarding towards augments installed in an entity.
/// </summary>
public sealed partial class AugmentSystem : EntitySystem
{
    private void RegisterStaminaEvents()
    {
        SubscribeLocalEvent<InstalledAugmentsComponent, StaminaDamageOnHitAttemptEvent>(OnStaminaHitAttempt);
        SubscribeLocalEvent<InstalledAugmentsComponent, StaminaMeleeHitEvent>(OnStaminaHit);
    }

    /// <summary>
    /// Handles when a stamina hit is attempted by a user with installed augments, forwarding the
    /// event to the augments so they have a chance to cancel the event if required.
    /// </summary>
    /// <param name="ent">The entity making the stamina attack attempt.</param>
    /// <param name="args">The args for the event.</param>
    private void OnStaminaHitAttempt(Entity<InstalledAugmentsComponent> ent, ref StaminaDamageOnHitAttemptEvent args)
    {
        ForwardEventToAugments(ent, ref args);
    }

    /// <summary>
    /// Handles when a stamina hit has been made against a target by a user with installed augments,
    /// forwarding the event to the augments so they have a chance to add stamina damage.
    /// </summary>
    /// <param name="ent">The user that has made the hit.</param>
    /// <param name="args">The args for the event.</param>
    private void OnStaminaHit(Entity<InstalledAugmentsComponent> ent, ref StaminaMeleeHitEvent args)
    {
        ForwardEventToAugments(ent, ref args);
    }
}
