namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// Event raised on an entity that is being mindbroken.
/// </summary>
[ByRefEvent]
public sealed class PsionicMindBrokenEvent(bool force)
{
    /// <summary>
    /// Whether even unremovable abilities should be removed.
    /// </summary>
    public readonly bool Force = force;

    /// <summary>
    /// This is true if at least one ability got removed.
    /// </summary>
    public bool Success;

    /// <summary>
    /// This remains true if ALL abilities were removed.
    /// </summary>
    public bool AllRemoved = true;
};
