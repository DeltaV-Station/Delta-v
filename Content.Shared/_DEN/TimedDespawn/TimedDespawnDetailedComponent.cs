using Robust.Shared.Audio;

namespace Content.Shared._DEN.TimedDespawn;

/// <summary>
/// This is used for a more detailed timed despawn component.
/// </summary>
[RegisterComponent]
public sealed partial class TimedDespawnDetailedComponent : Component
{
    [DataField]
    public TimeSpan StartTime { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// How long the entity will exist, in seconds, before despawning.
    /// </summary>
    [DataField]
    public float Lifetime = 5f;

    [DataField("examineText")]
    public LocId? ExamineLocId { get; set; } = "timed-despawn-holoprojection-examine";

    [DataField]
    public SoundSpecifier? StartSound { get; set; } = new SoundPathSpecifier("/Audio/_DEN/Effects/holofan_sound_looped.ogg");

    [DataField]
    public AudioParams StartSoundParams { get; set; } = AudioParams.Default;

    [DataField]
    public SoundSpecifier? EndSound { get; set; }

    [DataField]
    public AudioParams EndSoundParams { get; set; } = AudioParams.Default;
}
