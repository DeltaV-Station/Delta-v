using System.ComponentModel.DataAnnotations;
using Robust.Shared.Audio;
using Content.Server.Sound.Components;
using System;

namespace Content.Server.SimpleStation14.Silicon;

/// <summary>
///     Applies a <see cref="SpamEmitSoundComponent"/> to a Silicon when its battery is drained, and removes it when it's not.
/// </summary>
[RegisterComponent]
public sealed partial class SiliconEmitSoundOnDrainedComponent : Component
{
    [DataField("sound"), Required]
    public SoundSpecifier Sound = default!;

    [DataField("minInterval")]
    public TimeSpan Interval = TimeSpan.FromSeconds(8);

    [DataField("maxInterval")]
    public TimeSpan MaxInterval = TimeSpan.FromSeconds(15);

    [DataField("playChance")]
    public float PlayChance = 1f;

    [DataField("popUp")]
    public string? PopUp;
}
