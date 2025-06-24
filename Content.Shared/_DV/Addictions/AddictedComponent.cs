using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Addictions;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedAddictionSystem))]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class AddictedComponent : Component
{
    /// <summary>
    ///     Whether to suppress pop-ups.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Suppressed;

    /// <summary>
    ///     The <see cref="IGameTiming.CurTime"/> timestamp of last StatusEffect trigger.
    /// </summary>
    [DataField(serverOnly: true, customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan? LastMetabolismTime;

    /// <summary>
    ///     The <see cref="IGameTiming.CurTime"/> timestamp of the next popup.
    /// </summary>
    [DataField(serverOnly: true, customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan? NextEffectTime;

    /// <summary>
    ///     The <see cref="IGameTiming.CurTime"/> timestamp of the when the suppression ends
    /// </summary>
    [DataField(serverOnly: true, customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan? SuppressionEndTime;
}
