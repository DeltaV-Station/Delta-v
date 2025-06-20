namespace Content.Server.Objectives.Components;

/// <summary>
/// Sets the target for <see cref="TargetObjectiveComponent"/> to a random person.
/// </summary>
[RegisterComponent]
public sealed partial class PickRandomPersonComponent : Component
{
    /// <summary>
    /// DeltaV: If true a target must have a job with SetPreference set to true.
    /// </summary>
    [DataField]
    public bool OnlyChoosableJobs;
}
