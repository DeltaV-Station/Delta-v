using Robust.Shared.Prototypes;
using Content.Shared._Goobstation.StationEvents.Metric;

namespace Content.Shared._Goobstation.StationEvents;

/// <summary>
///   A series of named StoryBeats which we want to take the station through in the given sequence.
///   Gated by various settings such as the number of players
/// </summary>
[DataDefinition]
[Prototype("story")]
public sealed partial class StoryPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///   A human-readable description string for logging / admins
    /// </summary>
    [DataField]
    public string Description;

    /// <summary>
    ///   Minimum number of players on the station to pick this story
    /// </summary>
    [DataField]
    public int MinPlayers = -1;

    /// <summary>
    ///   Maximum number of players on the station to pick this story
    /// </summary>
    [DataField]
    public int MaxPlayers = Int32.MaxValue;

    /// <summary>
    ///   List of beat-ids in this story.
    /// </summary>
    [DataField]
    public ProtoId<StoryBeatPrototype>[]? Beats;
}
