using Content.Server._Goobstation.StationEvents.Metric;
using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Content.Shared._Goobstation.StationEvents; // DeltaV - shared the prototypes for our tests
using Content.Shared._Goobstation.StationEvents.Metric; // DeltaV - shared the prototypes for our tests

namespace Content.Server._Goobstation.StationEvents.Components;

[RegisterComponent, Access(typeof(GameDirectorSystem))]
public sealed partial class GameDirectorComponent : Component
{

    /// <summary>
    ///   How long until the next check for an event runs
    ///   Default value is how long until first event is allowed
    /// </summary>
    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan TimeNextEvent;

    /// <summary>
    ///   When the current beat started
    /// </summary>
    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan BeatStart;

    /// <summary>
    ///   The chaos we measured last time we ran
    ///   This is helpful for ViewVariables and perhaps as a cache to hold chaos for other functions to use.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ChaosMetrics CurrentChaos = new();

    /// <summary>
    ///   The story we are currently executing from stories (for easier debugging). Since it is for
    ///   debugging, it does not need a DataField.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<StoryPrototype> CurrentStoryName;

    /// <summary>
    ///   Remaining beats in the story we are currently executing (a list of beat IDs)
    /// </summary>
    [DataField]
    public List<ProtoId<StoryBeatPrototype>> RemainingBeats = new();

    /// <summary>
    /// Does this round start with multiple antags.
    /// </summary>
    [DataField]
    public bool DualAntags;

    /// <summary>
    /// Does this round start with antags at all?.
    /// </summary>
    [DataField]
    public bool NoRoundstartAntags;

    /// <summary>
    ///   Which stories the director can choose from (so we can change flavor of director by loading different stories)
    ///   One of these get picked randomly each time the current story is exhausted.
    /// </summary>
    [DataField]
    public ProtoId<StoryPrototype>[]? Stories;

    /// <summary>
    ///   A beat name we always use when we cannot find any stories to use.
    /// </summary>
    [DataField]
    public ProtoId<StoryBeatPrototype> FallbackBeatName = "Peace";

    /// <summary>
    ///   All the events that are allowed to run in the current story.
    /// </summary>
    [DataField]
    public List<PossibleEvent> PossibleEvents = new();
    // Could have Chaos multipliers here, or multipliers per player (so stories are harder with more players).
}

/// <summary>
///   Caches a possible StationEvent prototype with the chaos expected (from the game rule's data)
///   A list of PossibleEvents are built and cached by the game director.
/// </summary>
[DataDefinition]
public sealed partial class PossibleEvent
{
    /// <summary>
    ///   ID of a station event prototype (anomaly, spiders, pizzas, etc) that could occur
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId StationEvent;

    /// <summary>
    ///   Expected Chaos changes when this event occurs.
    ///   Used by the GameDirector, which picks an event expected to make the desired chaos changes.
    ///   Copy of the StationEventComponent.Chaos field from the relevant event.
    /// </summary>
    [DataField]
    public ChaosMetrics Chaos = new();

    // Begin DeltaV - Make Game Director VV Better
    public override string? ToString()
    {
        return $"{StationEvent} {Chaos}";
    }
    // End DeltaV - Make Game Director VV Better

    public PossibleEvent()
    {
    }

    public PossibleEvent(EntProtoId stationEvent, ChaosMetrics chaos)
    {
        StationEvent = stationEvent;
        Chaos = chaos;
    }
}
