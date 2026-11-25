namespace Content.Server.Psionics.Glimmer;

/// <summary>
/// Adds to glimmer at regular intervals. We'll use it for glimmer drains too when we get there.
/// </summary>
[RegisterComponent]
public sealed partial class GlimmerSourceComponent : Component
{

    [DataField] public float Accumulator = 0f;

    [DataField] public bool Active = true;

    /// <summary>
    /// Since glimmer is an int, we'll do it like this.
    /// </summary>
    [DataField] public float SecondsPerGlimmer = 10f;

    /// <summary>
    /// True if it produces glimmer, false if it subtracts it.
    /// </summary>
    [DataField] public bool AddToGlimmer = true;
}

