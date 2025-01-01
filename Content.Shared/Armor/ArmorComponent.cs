using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Armor;

/// <summary>
/// Used for clothing that reduces damage when worn.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedArmorSystem))]
public sealed partial class ArmorComponent : Component
{
    /// <summary>
    /// The damage reduction
    /// </summary>
    [DataField(required: true)]
    public DamageModifierSet Modifiers = default!;

    /// <summary>
    /// A multiplier applied to the calculated point value
    /// to determine the monetary value of the armor
    /// </summary>
    [DataField]
    public float PriceMultiplier = 1;

    /// <summary>
    /// DeltaV: The incoming stamina projectile damage will get multiplied by this value.
    /// </summary>
    [DataField]
    public float StaminaDamageCoefficient = 1;

    /// <summary>
    /// DeltaV: The configured stamina melee damage coefficient. The actual value used
    /// will be this or the blunt damage coefficient, whichever provides better protection.
    /// </summary>
    [DataField]
    private float _staminaMeleeDamageCoefficient = 1;

    /// <summary>
    /// DeltaV: Gets or sets the effective stamina melee damage coefficient, using either the configured
    /// value or the blunt damage coefficient, whichever provides better protection (lower value).
    /// </summary>
    [Access(typeof(SharedArmorSystem))]
    public float StaminaMeleeDamageCoefficient
    {
        get
        {
            // Try to get the blunt damage coefficient from modifiers
            var bluntCoefficient = Modifiers.Coefficients.GetValueOrDefault("Blunt", 1.0f);

            // Return whichever provides better protection (lower coefficient)
            return Math.Min(bluntCoefficient, _staminaMeleeDamageCoefficient);
        }
        set => _staminaMeleeDamageCoefficient = value;
    }
}

/// <summary>
/// Event raised on an armor entity to get additional examine text relating to its armor.
/// </summary>
/// <param name="Msg"></param>
[ByRefEvent]
public record struct ArmorExamineEvent(FormattedMessage Msg);
