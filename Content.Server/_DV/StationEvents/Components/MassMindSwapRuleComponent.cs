using Content.Server._DV.StationEvents.GameRules;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.StationEvents.Components;

[RegisterComponent, Access(typeof(MassMindSwapRule))]
public sealed partial class MassMindSwapRuleComponent : Component
{
    /// <summary>
    /// The mind swap is only temporary if true.
    /// </summary>
    [DataField]
    public bool IsTemporary;

    /// <summary>
    /// Whether this will also allow players to be mindswapped into NPCs.
    /// </summary>
    [DataField]
    public bool OnlyPlayers;

    /// <summary>
    /// Whether the mind swap will ignore mindshields.
    /// </summary>
    [DataField]
    public bool IgnoreMindshields = true;

    /// <summary>
    /// How long it'll take to activate after making the announcement.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(15);

    /// <summary>
    /// How long victims have to wait to swap back if <see cref="IsTemporary"/> is true.
    /// </summary>
    [DataField]
    public int ReturnSwapCooldown = 120;

    /// <summary>
    /// The FTL reference for what will be written in the announcement.
    /// </summary>
    [DataField]
    public string AnnouncementText = "mass-mind-swap-event-announcement";

    /// <summary>
    /// The FTL reference for what will the be the name of the sender.
    /// </summary>
    [DataField]
    public string AnnouncementSender = "mass-mind-swap-event-sender";

    /// <summary>
    /// The sound of the announcement warning of the imminent mind swap.
    /// </summary>
    [DataField]
    public SoundSpecifier AnnouncementSound = new SoundPathSpecifier("/Audio/Misc/notice1.ogg");


    /// <summary>
    ///The warning sound that will play just before the mind swap.
    /// </summary>
    [DataField]
    public SoundSpecifier SwapWarningSound = new SoundPathSpecifier("/Audio/_DV/Effects/clang2.ogg");

    /// <summary>
    /// The timestamp where the sound before the swap will be played.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? SoundTime;

    /// <summary>
    /// The timestamp where the swap will happen.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? SwapTime;

    /// <summary>
    /// How many pairs of people (2 in a pair) should be mind swapped.
    /// If null, all targets will be paired up.
    /// </summary>
    [DataField]
    public int? MaxNumberOfPairs;
}
