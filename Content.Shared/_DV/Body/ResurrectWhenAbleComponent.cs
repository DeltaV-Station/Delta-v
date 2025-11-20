using Robust.Shared.GameStates;

namespace Content.Shared._DV.Body;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ResurrectWhenAbleComponent : Component
{
    /// <summary>
    /// Time it takes to resurrect once the conditions are met (in seconds).
    /// If conditions are un-met during this time, the timer resets.
    /// </summary>
    [DataField]
    public float TimeToResurrect = 0f;
    /// <summary>
    /// What time to actually resurrect at. If null, we aren't resurrecting yet.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan? ResurrectAt = null;
    /// <summary>
    /// Text to show when examining the entity while it's resurrecting.
    /// </summary>
    [DataField]
    public LocId? ResurrectDesc = null;
}
