using Robust.Shared.Audio;

namespace Content.Shared._DV.MetalDetector;

/// <summary>
/// Responsible for holding some data of whom has passed by and which item is the most dangerous.
/// </summary>
[RegisterComponent]
public sealed partial class MetalDetectorComponent : Component
{
    [DataField]
    public float RunTime = 10.0f;

    [DataField]
    public float FalsePositiveChance = 5.0f;

    public float CurrentRunTime = 0.0f;

    public bool StartRunTime = false;

    [DataField]
    public SoundSpecifier? SirenSound;
}
