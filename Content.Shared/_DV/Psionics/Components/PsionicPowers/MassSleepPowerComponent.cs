using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MassSleepPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId => "ActionMassSleep";

    public override string PowerName => "psionic-power-name-mass-sleep";

    public override int MinGlimmerChanged => 10;

    public override int MaxGlimmerChanged => 20;

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
