using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
namespace Content.Shared._DV.Vision.Components;

/// <summary>
/// This is used for receiving psychological soothing from providers.
/// </summary>
[RegisterComponent][AutoGenerateComponentPause][AutoGenerateComponentState(fieldDeltas: true)][NetworkedComponent]
public sealed partial class PsychologicalSoothingReceiverComponent : Component
{
    /// <summary>
    /// The maximum range a provider can be from this receiver to provide soothing.
    /// </summary>
    [DataField]
    public float Range = 20f;

    /// <summary>
    /// The current amount of soothing this receiver has.
    /// </summary>
    [DataField][AutoNetworkedField]
    public float SoothedCurrent;

    /// <summary>
    /// The maximum amount of soothing this receiver can have.
    /// </summary>
    [DataField]
    public float SoothedMaximum = 1.0f;

    /// <summary>
    /// The minimum amount of soothing this receiver can have.
    /// </summary>
    [DataField]
    public float SoothedMinimum;

    /// <summary>
    /// The amount of soothing subtracted from the receiver every <see cref="SootheInterval"/>.
    /// </summary>
    [DataField]
    public float RateDecay = 0.01f;

    /// <summary>
    /// The amount of soothing added to the receiver every <see cref="SootheInterval"/> while in unobstructed range of a provider.
    /// </summary>
    [DataField]
    public float RateGrowth = 0.01f;

    /// <summary>
    /// The next game time at which the receiver will process soothing.
    /// </summary>
    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))][AutoPausedField][AutoNetworkedField]
    public TimeSpan? SootheNext;

    /// <summary>
    /// The interval at which the receiver will process soothing.
    /// </summary>
    [DataField]
    public TimeSpan SootheInterval = TimeSpan.FromSeconds(1);
}

[ByRefEvent]
public readonly record struct PsychologicalSoothingChanged(float Current, float Previous);
