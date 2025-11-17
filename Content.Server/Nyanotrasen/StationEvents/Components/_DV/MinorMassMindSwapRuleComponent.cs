using Content.Server.Nyanotrasen.StationEvents.Events._DV;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Nyanotrasen.StationEvents.Components._DV;

[RegisterComponent, Access(typeof(MinorMassMindSwapRule))]
public sealed partial class MinorMassMindSwapRuleComponent : Component
{
    /// <summary>
    /// The mind swap is only temporary if true.
    /// </summary>
    [DataField("isTemporary")]
    public bool IsTemporary;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(60);

    [DataField]
    public SoundSpecifier AnnouncementSound = new SoundPathSpecifier("/Audio/Misc/notice1.ogg");

    [DataField]
    public SoundSpecifier SwapWarningSound = new SoundPathSpecifier("/Audio/_DV/Effects/clang2.ogg");

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? SoundTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? SwapTime;
}
