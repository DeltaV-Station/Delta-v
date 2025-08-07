using Content.Server.Nyanotrasen.StationEvents.Events;
using Robust.Shared.Audio;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(MassMindSwapRule))]
public sealed partial class MassMindSwapRuleComponent : Component
{
    /// <summary>
    /// The mind swap is only temporary if true.
    /// </summary>
    [DataField("isTemporary")]
    public bool IsTemporary;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(15);

    [DataField]
    public SoundSpecifier AnnouncementSound = new SoundPathSpecifier("/Audio/Misc/notice1.ogg");

    [DataField]
    public SoundSpecifier SwapWarningSound = new SoundPathSpecifier("/Audio/_DV/Effects/clang2.ogg");

    public TimeSpan StartTime;

    public bool Started = false;

    public bool PlayedWarningSound = false;

}
