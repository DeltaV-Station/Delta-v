using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

/// <summary>
/// This is the component for the psionic power Indomitable Will.
/// Any entity with it is capable of using that power.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IndomitableWillPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionIndomitableWill";

    public override string PowerName { get; set; } = "psionic-power-name-indomitable-will";

    public override int MinGlimmerChanged { get; set; } = 10;

    public override int MaxGlimmerChanged { get; set; } = 30;

    /// <summary>
    /// The duration of the buff when the button is pressed when nothing unusual is happening.
    /// </summary>
    [DataField]
    public TimeSpan BuffDuration = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How much reagent will be removed when the power is activated.
    /// </summary>
    [DataField]
    public FixedPoint2 ReagentRemoved = 50f;

    /// <summary>
    /// The duration of the buff when the button is pressed while in critical state.
    /// </summary>
    [DataField]
    public TimeSpan CritBuffDuration = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The action delay when used in without anything critical going on.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan? NormalUseDelay;

    /// <summary>
    /// The action delay when used in special occasions, such as uncuffing or critical state.
    /// </summary>
    [DataField]
    public TimeSpan SpecialUsageCooldown = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Damage dealt to the psionic user when uncuffing themselves.
    /// </summary>
    [DataField]
    public DamageSpecifier UncuffDamage = new()
    {
        DamageDict = new ()
        {
            { "Blunt", 15 },
        },
    };
}
