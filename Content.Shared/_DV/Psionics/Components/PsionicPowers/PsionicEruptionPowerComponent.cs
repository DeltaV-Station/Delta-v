using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PsionicEruptionPowerComponent : BasePsionicPowerComponent
{
    /// <summary>
    /// The prototype ID for the action.
    /// It's set up in the YML and then referenced via a string here.
    /// </summary>
    public override EntProtoId ActionProtoId => "ActionEruption";

    /// <summary>
    /// The Loc string for the name of the power.
    /// </summary>
    public override string PowerName => "psionic-power-name-eruption";

    /// <summary>
    /// The minimum glimmer amount that will be changed upon use of the psionic power.
    /// Should be lower than <see cref="MaxGlimmerChanged"/>.
    /// </summary>
    public override int MinGlimmerChanged => -200;

    /// <summary>
    /// The maximum glimmer amount that will be changed upon use of the psionic power.
    /// Should be higher than <see cref="MinGlimmerChanged"/>.
    /// </summary>
    public override int MaxGlimmerChanged => -100;

    /// <summary>
    /// Minimum time for the Detonation DoAfter to take effect.
    /// Half of it is the time for every spark.
    /// </summary>
    [DataField]
    public TimeSpan MinDetonateDelay = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Maximum time for the Detonation DoAfter to take effect.
    /// Half of it is the time for every spark.
    /// </summary>
    [DataField]
    public TimeSpan MaxDetonateDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The ID of the DoAfter that is used for the detonation of the psionic.
    /// This is used to check if the psionic is currently detonating.
    /// </summary>
    /// <returns>Null, if psionic isn't detonating. A valid UID if otherwise.</returns>
    public DoAfterId? DoAfterId;

    /// <summary>
    /// The sound that appears when the action is pressed.
    /// </summary>
    [DataField]
    public SoundSpecifier SoundUse = new SoundPathSpecifier("/Audio/Nyanotrasen/Psionics/heartbeat_fast.ogg");

    /// <summary>
    /// The timespan for the next annoy popup. This will be refreshed depending on the glimmer amount.
    /// </summary>
    [DataField]
    public TimeSpan NextAnnoy = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The timespan for the next spark to appear.
    /// </summary>
    [DataField]
    public TimeSpan NextSpark = TimeSpan.MaxValue;
}
