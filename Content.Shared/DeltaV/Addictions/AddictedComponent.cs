using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.DeltaV.Addictions;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class AddictedComponent : Component
{
    [DataField, AutoNetworkedField]
    public float AddictionStrength = 0f;

    /// <summary>
    ///     Setting this to true will suppress any pop-ups.
    /// </summary>
    [DataField]
    public bool Suppressed;

    /// <summary>
    ///     The <see cref="IGameTiming.CurTime"/> timestamp of last StatusEffect trigger.
    /// </summary>
    [DataField(serverOnly: true)]
    public TimeSpan? LastMetabolismTime;
}
