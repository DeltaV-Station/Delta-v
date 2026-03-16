using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PrecognitionPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionPrecognition";

    public override string PowerName { get; set; } = "psionic-power-name-precognition";

    public override int MinGlimmerChanged { get; set; } = 5;

    public override int MaxGlimmerChanged { get; set; } = 10;

    /// <summary>
    /// This dictates the chance that it'll return a wrong message, seeding unreliance.
    /// </summary>
    [DataField]
    public float RandomResultChance = 0.2f;

    /// <summary>
    /// The time it takes for the precognition to finish. Casting time, so to speak.
    /// </summary>
    [DataField]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(8.35); // The length of the sound effect

    /// <summary>
    /// It'll have a shorter cooldown if it was cancelled.
    /// </summary>
    [DataField]
    public TimeSpan CancellationCooldown = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The minimum time distance to the next event to be considered as a result for the precognition.
    /// </summary>
    [DataField]
    public TimeSpan MinEventTimeDistance = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The maximum time distance to the next event to be considered as a result for the precognition.
    /// </summary>
    [DataField]
    public TimeSpan MaxEventTimeDistance = TimeSpan.FromMinutes(10);

    /// <summary>
    /// The sound that plays when you start the DoAfter.
    /// </summary>
    [DataField]
    public SoundSpecifier VisionSound = new SoundPathSpecifier("/Audio/_DV/Effects/clang2.ogg");

    /// <summary>
    /// Cached SoundStream, so it can be stopped if the DoAfter is prematurely cancelled.
    /// </summary>
    [DataField]
    public EntityUid? SoundStream;
}
