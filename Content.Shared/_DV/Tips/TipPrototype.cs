using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Tips;

/// <summary>
/// Prototype for dynamic tips shown to players after spawning.
/// Tips can have conditions and delays before appearing.
/// </summary>
[Prototype]
public sealed partial class TipPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Localization key for the tip title.
    /// </summary>
    [DataField(required: true)]
    public LocId Title;

    /// <summary>
    /// Localization key for the tip description/body text.
    /// Supports rich text formatting.
    /// </summary>
    [DataField(required: true)]
    public LocId Description;

    /// <summary>
    /// Delay after player spawns before showing this tip.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Priority for tip ordering. Lower values show first when multiple tips are queued.
    /// </summary>
    [DataField]
    public int Priority;

    /// <summary>
    /// Sound to play when the tip is shown.
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Effects/voteding.ogg");

    /// <summary>
    /// Conditions that must ALL be met for this tip to be shown.
    /// If empty, the tip is always shown.
    /// </summary>
    [DataField]
    public List<Conditions.TipCondition> Conditions = new();
}
