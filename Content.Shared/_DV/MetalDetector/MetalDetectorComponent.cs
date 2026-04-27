using Robust.Shared.Audio;
using Robust.Shared.Serialization;

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

[Serializable, NetSerializable]
public enum MetalDetectorVisuals : byte
{
    MetalDetectorActivated,
}

[Serializable, NetSerializable]
public enum MetalDetectorVisualLayers : byte
{
    MetalDetectorLayer,
}
