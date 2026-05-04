using Content.Shared.DeviceLinking;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._DV.MetalDetector;

/// <summary>
/// Responsible for holding some data of whom has passed by and which item is the most dangerous.
/// </summary>
[RegisterComponent]
public sealed partial class MetalDetectorComponent : Component
{
    [DataField]
    public TimeSpan SirenRunTime = TimeSpan.FromSeconds(10);

	/// <summary>
	/// Value to determine tha chance that the Metal Detecto simply fires and gives a false positive. from 0 to 100.
	/// </summary>
    [DataField]
    public float FalsePositiveChance = 5.0f;

    /// <summary>
	/// Timespan which is set runtime to determine when the Siren should stop running
	/// </summary>
	[DataField]
    public TimeSpan EndOfSirenSound = TimeSpan.FromSeconds(0);

    public bool IsSirenRunning = false;

	/// <summary>
	/// Siren Sound which is played when the Metal Detector fires.
	/// </summary>
    [DataField]
    public SoundSpecifier? SirenSound;

    /// <summary>
    ///     The port that gets signaled when the the metal detector fires
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<SourcePortPrototype>))]
    public string TriggerPort = "Trigger";
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
