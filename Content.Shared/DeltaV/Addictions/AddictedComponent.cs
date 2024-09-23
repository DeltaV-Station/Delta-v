using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.DeltaV.Addictions;

[RegisterComponent, NetworkedComponent]
public sealed partial class AddictedComponent : Component
{
    /// <summary>
    ///     Whether to suppress pop-ups.
    /// </summary>
    [DataField]
    public bool Suppressed;

    /// <summary>
    ///     The <see cref="IGameTiming.CurTime"/> timestamp of last StatusEffect trigger.
    /// </summary>
    [DataField(serverOnly: true)]
    public TimeSpan? LastMetabolismTime;
}
