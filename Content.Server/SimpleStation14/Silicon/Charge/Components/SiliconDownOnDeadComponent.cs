using System.Threading;

namespace Content.Server.SimpleStation14.Silicon.Death;

/// <summary>
///     Marks a Silicon as becoming incapacitated when they run out of battery charge.
/// </summary>
/// <remarks>
///     Uses the Silicon System's charge states to do so, so make sure they're a battery powered Silicon.
/// </remarks>
[RegisterComponent]
public sealed class SiliconDownOnDeadComponent : Component
{
    /// <summary>
    ///     Cancellation token for the silicon's wake timer.
    /// </summary>
    public CancellationTokenSource? WakeToken { get; set; }

    /// <summary>
    ///     The time it will take for a Silicon to "wake up" after leaving the Dead state, in seconds.
    /// </summary>
    /// <remarks>
    ///     If not zero, the Silicon will not actually come back to life until after this much time has passed.
    ///     This can prevent 'flickering' between the two states.
    /// </remarks>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("deadBuffer")]
    public float DeadBuffer { get; set; } = 2.5f;

    /// <summary>
    ///     Is this Silicon currently dead?
    /// </summary>
    public bool Dead { get; set; } = false;
}
