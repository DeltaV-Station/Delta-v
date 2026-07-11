using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._ES.Sparks.Components;

/// <summary>
/// Component that tracks cooldown for spawning sparks on an entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
[Access(typeof(ESSparksSystem))]
public sealed partial class ESSparkCooldownComponent : Component
{
    /// <summary>
    /// Minimum time inbetween sparks occuring from hits.
    /// Used to reduce spark spam.
    /// </summary>
    [DataField]
    public TimeSpan SparkDelay = TimeSpan.FromSeconds(0.25f);

    /// <summary>
    /// The last time that a spark occured.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan LastSparkTime;
}
