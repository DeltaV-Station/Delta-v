using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MassSleepPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionMassSleep";

    public override string PowerName { get; set; } = "psionic-power-name-mass-sleep";

    public override int MinGlimmerChanged { get; set; } = 30;

    public override int MaxGlimmerChanged { get; set; } = 60;

    /// <summary>
    /// The radius to where people will fall asleep.
    /// </summary>
    [DataField]
    public float Radius = 2f;

    /// <summary>
    /// The duration of the DoAfter. Casting time, per say.
    /// </summary>
    [DataField]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(4);

    /// <summary>
    /// How long the victims will be asleep.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The radius for where people will be warned about being mass slept.
    /// </summary>
    [DataField]
    public float WarningRadius = 6f;
}
