using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AltruisticHemomancyPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionAltruisticHemomancy";

    public override string PowerName { get; set; } = "Altruistic Hemomancy";

    public override int MinGlimmerChanged { get; set; } = 5;

    public override int MaxGlimmerChanged { get; set; } = 20;

    /// <summary>
    /// The normal DoAfterDuration of healing.
    /// </summary>
    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The modifier for the DoAfterDuration if the target is in critical state.
    /// </summary>
    [DataField]
    public float CriticalDoAfterDurationModifier = 0.5f;

    /// <summary>
    /// The modifier applied to the blood cost when healing a target in critical state.
    /// </summary>
    [DataField]
    public float CriticalHealingCostModifier = 1.25f;

    /// <summary>
    /// The standard blood cost of using the power. It's 1/10th of a human entire blood reserves.
    /// </summary>
    [DataField]
    public float BloodCost = -30f;

    /// <summary>
    /// The minimum blood percentage someone has to have to use the power.
    /// </summary>
    [DataField]
    public float MinBloodPercentage = 0.4f;

    /// <summary>
    /// The healing applied evenly to the target with each DoAfter.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<DamageGroupPrototype>, FixedPoint2> Heal = new()
    {
        { "Brute", -20 },
        { "Burn", -20 },
        { "Airloss", -20 },
        { "Toxin", -20 },
    };

    /// <summary>
    /// The target's bleeding reduced with every healing tick.
    /// </summary>
    [DataField]
    public float ReducedBleeding = -4f;

    /// <summary>
    /// How many times a target can be healed with the one ability usage.
    /// </summary>
    [DataField]
    public int MaxHealingTicks = 3;

    /// <summary>
    /// The current counter of completed healing.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public int TickCounter = 0;

    /// <summary>
    /// The target's rot counter reduced with every healing tick.
    /// </summary>
    [DataField]
    public TimeSpan ReduceRot = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The Damage the psionic user receives everytime they heal targets.
    /// </summary>
    [DataField]
    public DamageSpecifier DamageOnHealing = new()
    {
        DamageDict = new()
        {
            { "Bloodloss", 10 },
        },
    };

    /// <summary>
    /// The Damage the psionic user receives everytime they heal dead targets.
    /// </summary>
    [DataField]
    public DamageSpecifier DamageOnHealingRot = new()
    {
        DamageDict = new()
        {
            { "Cellular", 30 },
        },
    };

    /// <summary>
    /// The modifier applied to the blood cost when healing a dead target.
    /// </summary>
    [DataField]
    public float RotHealingCostModifier = 2f;

    /// <summary>
    /// The modifier for the DoAfterDuration if the target is dead.
    /// </summary>
    [DataField]
    public float RotDoAfterDurationModifier = 2f;
}
