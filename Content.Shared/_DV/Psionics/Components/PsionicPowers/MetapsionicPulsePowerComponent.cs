using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MetapsionicPulsePowerComponent : BasePsionicPowerComponent
{
    /// <summary>
    /// The prototype ID for the action.
    /// It's set up in the YML and then referenced via a string here.
    /// </summary>
    public override EntProtoId ActionProtoId => "ActionMetapsionicPulse";

    /// <summary>
    /// The Loc string for the name of the power.
    /// </summary>
    public override string PowerName => "psionic-power-name-metapsionic";

    /// <summary>
    /// The minimum glimmer amount that will be changed upon use of the psionic power.
    /// Should be lower than <see cref="MaxGlimmerChanged"/>.
    /// </summary>
    public override int MinGlimmerChanged => 2;

    /// <summary>
    /// The maximum glimmer amount that will be changed upon use of the psionic power.
    /// Should be higher than <see cref="MinGlimmerChanged"/>.
    /// </summary>
    public override int MaxGlimmerChanged => 4;

    /// <summary>
    /// The radius of the pulse.
    /// </summary>
    [DataField]
    public float Range = 1.5f;
}
