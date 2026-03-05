using Content.Shared.Damage;
using Content.Shared.Inventory; // Shitmed Change
using Content.Shared.Humanoid; // Shitmed Change
using Content.Shared.Damage.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Standing;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.Body.Systems;

public abstract partial class SharedBodySystem : EntitySystem
{
    /*
     * See the body partial for how this works.
     */

    /// <summary>
    /// Container ID prefix for any body parts.
    /// </summary>
    public const string PartSlotContainerIdPrefix = "body_part_slot_";

    /// <summary>
    /// Container ID for the ContainerSlot on the body entity itself.
    /// </summary>
    public const string BodyRootContainerId = "body_root_part";

    /// <summary>
    /// Container ID prefix for any body organs.
    /// </summary>
    public const string OrganSlotContainerIdPrefix = "body_organ_slot_";

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] protected readonly IPrototypeManager Prototypes = default!;
    [Dependency] protected readonly DamageableSystem Damageable = default!;
    [Dependency] protected readonly MovementSpeedModifierSystem Movement = default!;
    [Dependency] protected readonly SharedContainerSystem Containers = default!;
    [Dependency] protected readonly SharedTransformSystem SharedTransform = default!;
    [Dependency] protected readonly StandingStateSystem Standing = default!;
    // Shitmed Change - the systems below are named like private dependencies because I want to reduce the amount of
    // conflicts and for some reason, shitmed uses partial classes instead of being its own thing.
#pragma warning disable IDE1006 // Ignore Naming Style Issues
    [Dependency] protected readonly InventorySystem _inventory = default!;
    [Dependency] protected readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
#pragma warning restore IDE1006

    public override void Initialize()
    {
        base.Initialize();

        InitializeBody();
        InitializeParts();
        InitializeOrgans();
        // Shitmed Change Start
        // To try and mitigate the server load due to integrity checks, we set up a Job Queue.
        InitializeIntegrityQueue();
        InitializePartAppearances();
        // Shitmed Change End
        InitializeGibDirtying(); // DeltaV
    }

    /// <summary>
    /// Inverse of <see cref="GetPartSlotContainerId"/>
    /// </summary>
    protected static string? GetPartSlotContainerIdFromContainer(string containerSlotId)
    {
        // This is blursed
        var slotIndex = containerSlotId.IndexOf(PartSlotContainerIdPrefix, StringComparison.Ordinal);

        if (slotIndex < 0)
            return null;

        var slotId = containerSlotId.Remove(slotIndex, PartSlotContainerIdPrefix.Length);
        return slotId;
    }

    /// <summary>
    /// Gets the container Id for the specified slotId.
    /// </summary>
    public static string GetPartSlotContainerId(string slotId)
    {
        return PartSlotContainerIdPrefix + slotId;
    }

    /// <summary>
    /// Gets the container Id for the specified slotId.
    /// </summary>
    public static string GetOrganContainerId(string slotId)
    {
        return OrganSlotContainerIdPrefix + slotId;
    }
}
