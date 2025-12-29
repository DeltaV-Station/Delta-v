using Content.Shared._DV.Kitchen.Systems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Nutrition;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Kitchen.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedDeepFryerSystem))]
public sealed partial class DeepFryerComponent : Component
{
    /// <summary>
    /// The <see cref="EntityWhitelist"/> of items that fit into the deep fryer
    /// </summary>
    [DataField]
    public EntityWhitelist Whitelist = new();

    /// <summary>
    /// The <see cref="EntityWhitelist"/> of items that will never fit into the deep fryer, even if on the whitelist
    /// </summary>
    [DataField]
    public EntityWhitelist Blacklist = new();

    /// <summary>
    /// The <see cref="Solution"/> of cooking oil
    /// </summary>
    [DataField]
    public string Solution = "vat_oil";

    /// <summary>
    /// The allowed reagents that will not cause a reaction when inserted
    /// </summary>
    [DataField]
    public List<ProtoId<ReagentPrototype>> FryingOils = new();

    /// <summary>
    /// The container that holds items being fried
    /// </summary>
    [DataField]
    public string ContainerName = "fryer_basket";

    /// <summary>
    /// Maximum number of items that can be in the fryer at once
    /// </summary>
    [DataField]
    public int MaxItems = 3;

    /// <summary>
    /// Minimum amount of oil required to fry items (in units)
    /// </summary>
    [DataField]
    public FixedPoint2 MinimumOilVolume = 25;

    /// <summary>
    /// Tracks cooking progress for each item in the fryer
    /// Key: EntityUid of the item being fried
    /// Value: CookingItem struct containing recipe and time
    /// </summary>
    [DataField]
    public Dictionary<EntityUid, CookingItem> CookingItems = new();

    /// <summary>
    /// The <see cref="SoundSpecifier"/> that will play once an item finishes cooking
    /// </summary>
    [DataField]
    public SoundSpecifier FinishedCookingSound = new SoundPathSpecifier("/Audio/Machines/Nuke/angry_beep.ogg");

    /// <summary>
    /// The <see cref="SoundSpecifier"/> that will play once an item finishes burning
    /// </summary>
    [DataField]
    public SoundSpecifier FinishedBurningSound = new SoundPathSpecifier("/Audio/Effects/drop.ogg");

    /// <summary>
    /// Current oil quality as a value from 0.0 (0%) to 1.0 (100%)
    /// </summary>
    [DataField, AutoNetworkedField]
    public float OilQuality = 1.0f;

    /// <summary>
    /// How much the oil quality degrades per recipe cooked (as a percentage, e.g., 0.05 = 5%)
    /// </summary>
    [DataField]
    public float OilDegradationPerRecipe = 0.05f;

    /// <summary>
    /// Degradation multiplier when at minimum oil volume (higher = faster degradation with less oil)
    /// </summary>
    [DataField]
    public float MinOilVolumeDegradationMultiplier = 4.0f;

    /// <summary>
    /// How much quality is restored per unit of oil added (e.g., 0.01 = 1% quality per unit)
    /// </summary>
    [DataField]
    public float OilQualityRestorationPerUnit = 0.01f;

    /// <summary>
    /// Chance (0.0 to 1.0) that BurnedResult will spawn when using Foul quality oil
    /// </summary>
    [DataField]
    public float FoulOilBurnChance = 0.3f;

    /// <summary>
    /// Maps oil quality levels to the flavors that should be added to cooked items
    /// </summary>
    [DataField]
    public Dictionary<OilQuality, List<ProtoId<FlavorPrototype>>> OilQualityFlavors = new();

    /// <summary>
    /// Time tolerance for multi-ingredient recipes (ingredients must be inserted within this window)
    /// </summary>
    [DataField]
    public TimeSpan CookingTolerance = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The time it will take for ingredients to start burning if not a part of any recipe
    /// </summary>
    [DataField]
    public TimeSpan BaseBurnTime = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The default result for when something burns
    /// </summary>
    [DataField]
    public EntProtoId BaseBurnedResult = "FoodBadRecipe";

    /// <summary>
    /// Chance (0.0 to 1.0) that a thrown item will miss the fryer and land nearby instead.
    /// Professional chefs always have a 0% miss chance regardless of this value.
    /// </summary>
    /// <seealso cref="ProfessionalChefComponent"/>
    [DataField]
    public float MissChance = 0.25f;
}

/// <summary>
/// Tracks an item being cooked in the deep fryer
/// </summary>
[DataDefinition]
public partial record struct CookingItem
{
    /// <summary>
    /// The deep fryer recipe being used to cook this item
    /// </summary>
    [DataField]
    public ProtoId<DeepFryerRecipePrototype>? Recipe;

    /// <summary>
    /// When this item started cooking or burning
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan TimeStarted;

    /// <summary>
    /// Whether this item has finished cooking and is now burning
    /// </summary>
    [DataField]
    public bool IsBurning;

    public CookingItem(ProtoId<DeepFryerRecipePrototype>? recipe, TimeSpan timeStarted, bool isBurning = false)
    {
        Recipe = recipe;
        TimeStarted = timeStarted;
        IsBurning = isBurning;
    }
}

/// <summary>
/// Represents the quality level of oil in the deep fryer
/// </summary>
[Serializable, NetSerializable]
public enum OilQuality : byte
{
    Pristine,  // >= 90%
    Clean,     // >= 70%
    Used,      // >= 50%
    Dirty,     // >= 30%
    Foul       // >= 0%
}

[Serializable, NetSerializable]
public enum DeepFryerVisuals : byte
{
    Bubbling
}
