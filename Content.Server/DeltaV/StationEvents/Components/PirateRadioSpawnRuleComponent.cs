using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(PirateRadioSpawnRule))]
public sealed partial class PirateRadioSpawnRuleComponent : Component
{
    [DataField("PirateRadioShuttlePath")]
    public string PirateRadioShuttlePath = "Maps/Shuttles/DeltaV/DV-pirateradio.yml";

    [DataField("additionalRule")]
    public EntityUid? AdditionalRule;

    [DataField("debrisCount")]
    public int DebrisCount { get; set; }

    [DataField("distanceModifier")]
    public float DistanceModifier { get; set; }

    [DataField("debrisDistanceModifier")]
    public float DebrisDistanceModifier { get; set; }
}
