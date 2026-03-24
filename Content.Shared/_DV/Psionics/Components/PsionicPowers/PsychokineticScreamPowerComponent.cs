using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PsychokineticScreamPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionPsychokineticScream";

    public override string PowerName { get; set; } = "psionic-power-name-psychokinetic";

    public override int MinGlimmerChanged { get; set; } = 20;

    public override int MaxGlimmerChanged { get; set; } = 30;

    /// <summary>
    /// The radius in which lights will be broken.
    /// </summary>
    [DataField]
    public float Radius = 10f;

    /// <summary>
    /// If true, lights will only be broken if the entity has line of sight to them.
    /// </summary>
    [DataField]
    public bool LineOfSight = true;

    /// <summary>
    /// The radius to ignore line of sight restrictions.
    /// </summary>
    [DataField]
    public float PenetratingRadius;

    /// <summary>
    /// The sound to play when the ability is used.
    /// </summary>
    [DataField]
    public SoundSpecifier AbilitySound = new SoundPathSpecifier("/Audio/_DV/Effects/creepyshriek.ogg");

    /// <summary>
    /// The effect to spawn when the ability is used.
    /// </summary>
    [DataField]
    public EntProtoId Effect = "CMEffectScreech";
}
