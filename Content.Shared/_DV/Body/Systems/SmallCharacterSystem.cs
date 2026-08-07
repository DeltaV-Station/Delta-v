using Content.Shared._DV.Body.Components;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Item;
using JetBrains.Annotations;
using Robust.Shared.Physics.Components;

namespace Content.Shared._DV.Body.Systems;

/// <summary>
/// Used to relay or subscribe to events if a character's scale is 1.0 or below.
/// This is only used for the height slider scale.
/// </summary>
public sealed partial class SmallCharacterSystem : EntitySystem
{
    private const float NO_PENALTY = 1.0f;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawn);
    }

    private void OnSpawn(PlayerSpawnCompleteEvent ev)
    {
        if (TryComp<HumanoidProfileComponent>(ev.Mob, out var profile))
            ApplySmallCharacter(ev.Mob, profile.Height);
    }

    [PublicAPI]
    public float ApplyPullSpeedPenalty(Entity<SmallCharacterComponent?> puller, EntityUid? pulledEntity)
    {
        // Ignore if they aren't pulling anything...
        if (!pulledEntity.HasValue)
            return NO_PENALTY;

        // Ignore if they aren't a small character in the first place
        if (!Resolve(puller, ref puller.Comp, false))
            return NO_PENALTY;

        // If the pulled entity has the component that ignores the penalty
        if (HasComp<UnaffectedBySizePenaltyComponent>(pulledEntity))
            return NO_PENALTY;

        // Ignore if it's an item that can be held or stored. It would be weird to
        // slow by X% from pulling a piece of paper or a gun when you can just hold it
        // and not suffer from a penalty.
        if (HasComp<ItemComponent>(pulledEntity))
            return NO_PENALTY;

        // Ignore if the object is floating in the air.
        if (TryComp<PhysicsComponent>(pulledEntity, out var pulledPhysics)
                && pulledPhysics.BodyStatus == BodyStatus.InAir)
            return NO_PENALTY;

        return puller.Comp.PullSpeedPenalty;
    }

    #region Static Members
    /// <summary>
    /// Gets the move-speed penalty as a float. Should be applied multiplicatively.
    /// Caps at 1 so we don't make bigger characters faster when pulling.
    /// </summary>
    /// <returns></returns>
    [PublicAPI]
    public static float GetPullSpeedPenaltyFromScale(float scale = 1.0f)
    {
        return Math.Min(scale * scale, 1);
    }

    /// <summary>
    /// Calculates a well-formed display string of the pull speed penalty.
    /// Used primarily in the character editor to get the well-formed percent
    /// without having to duplicate formulas.
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    [PublicAPI]
    public static string GetPullSpeedPenaltyDisplayFromScale(float scale = 1.0f)
    {
        return $"{Math.Round((1 - GetPullSpeedPenaltyFromScale(scale)) * 100)}%";
    }
    #endregion

    #region Private Members
    private void ApplySmallCharacter(EntityUid uid, float scale = 1)
    {
        if (scale >= 1)
            return;

        // The character scale is stored in the HumanoidProfileComponent if you ever
        // need it.
        var comp = EnsureComp<SmallCharacterComponent>(uid);
        comp.PullSpeedPenalty = GetPullSpeedPenaltyFromScale(scale);
        Dirty(uid, comp);
    }
    #endregion
}
