using Content.Server._DV.StationEvents.GameRules;

namespace Content.Server._DV.StationEvents.Components;

[RegisterComponent, Access(typeof(NoosphericStormRule))]
public sealed partial class NoosphericStormRuleComponent : Component
{
    /// <summary>
    /// The minimum amount of psionics that are created with each storm.
    /// </summary>
    [DataField]
    public int MinAwaken = 1;

    /// <summary>
    /// The maximum amount of psionics that are created with each storm.
    /// </summary>
    [DataField]
    public int MaxAwaken = 3;

    ///<summary>
    /// The coefficient for additional psionic awakenings.
    /// It's calculated via glimmer / coefficient, rounded down.
    /// </summary>
    [DataField]
    public float AdditionalAwokenPerGlimmer = 100f;

    /// <summary>
    /// The minimum amount of glimmer added.
    /// </summary>
    [DataField]
    public int BaseGlimmerAddMin = 65;

    /// <summary>
    /// The maximum amount of glimmer added.
    /// </summary>
    [DataField]
    public int BaseGlimmerAddMax = 85;

    /// <summary>
    /// Multiply the EventSeverityModifier by this to determine how much extra glimmer to add.
    /// </summary>
    [DataField]
    public float GlimmerSeverityCoefficient = 0.25f;
}
