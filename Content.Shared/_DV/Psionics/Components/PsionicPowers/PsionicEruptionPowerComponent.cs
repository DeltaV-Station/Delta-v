using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class PsionicEruptionPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId => "ActionEruption";

    public override string PowerName => "psionic-power-name-eruption";

    public override int MinGlimmerChanged => -200;

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
    /// The sound that appears when the action is pressed.
    /// </summary>
    [DataField]
    public SoundSpecifier SoundUse = new SoundPathSpecifier("/Audio/Nyanotrasen/Psionics/heartbeat_fast.ogg");

    /// <summary>
    /// The timespan for the next annoy popup. This will be refreshed depending on the glimmer amount.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextAnnoy = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The timespan for the next spark to appear.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextSpark = TimeSpan.MaxValue;
}
