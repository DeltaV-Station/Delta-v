using Content.Server._DV.StationEvents.Events;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.StationEvents.Components;

[RegisterComponent, Access(typeof(MinorMassMindSwapRule))]
public sealed partial class MinorMassMindSwapRuleComponent : Component
{
    /// <summary>
    /// The mind swap is only temporary if true.
    /// </summary>
    [DataField]
    public bool IsTemporary = false;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(60);

    [DataField]
    public int ReturnSwapCooldown = 120;

    [DataField]
    public SoundSpecifier AnnouncementSound = new SoundPathSpecifier("/Audio/Misc/notice1.ogg");

    [DataField]
    public SoundSpecifier SwapWarningSound = new SoundPathSpecifier("/Audio/_DV/Effects/clang2.ogg");

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? SoundTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? SwapTime;

    [DataField]
    public int MaxNumberOfPairs = 3;
}
