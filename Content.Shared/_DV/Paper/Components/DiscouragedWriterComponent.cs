using Robust.Shared.GameStates;

namespace Content.Shared._DV.Paper.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class DiscouragedWriterComponent : Component
{
    /// <summary>
    /// The current counter of stacks.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int PunishStacks;

    /// <summary>
    /// The current counter of writing attempts.
    /// Each attempt increments the counter and incurs a <see cref="Delay"/> before attempting to write again.
    /// When this is higher than <see cref="PunishStacks"/>, the attempt to write is successful and clears the attempts.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int PreviousAttempts;

    /// <summary>
    /// When the next <see cref="PunishStacks"/> decays.
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextStackDecay = TimeSpan.Zero;

    /// <summary>
    /// The time of the last writing attempt.
    /// This determines when to decay <see cref="AttemptDecay"/>. Additionally, successfully writing clears it too.
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan LastAttemptTime = TimeSpan.Zero;

    /// <summary>
    /// The delay after which all <see cref="PreviousAttempts"/> get cleared.
    /// </summary>
    [DataField]
    public TimeSpan AttemptDecay = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How much each <see cref="PunishStacks"/> delays an attempt to write on a paper.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How long it takes for <see cref="PunishStacks"/> to start decaying after gaining one.
    /// </summary>
    /// <remarks> This refreshes when gaining a new <see cref="PunishStacks"/>. </remarks>
    [DataField]
    public TimeSpan DecayDelay = TimeSpan.FromMinutes(1);

    /// <summary>
    /// The time for each stack to decay after the <see cref="DecayDelay"/> ran out.
    /// </summary>
    [DataField]
    public TimeSpan DecayInterval = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The LocIds for the popups that show up on unsuccessful attempts.
    /// </summary>
    [DataField]
    public List<LocId> PopupLocIds = new()
    {
        "comp-discouraged-mime-popup-1",
        "comp-discouraged-mime-popup-2",
        "comp-discouraged-mime-popup-3",
        "comp-discouraged-mime-popup-4",
        "comp-discouraged-mime-popup-5",
    };
}
