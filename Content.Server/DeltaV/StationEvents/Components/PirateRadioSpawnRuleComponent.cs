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

    /// <summary>
    /// "Stations of Unusual Size Constant", derived from the AABB.Width of Shoukou.
    /// This Constant is used to check the size of a station relative to the reference point
    /// </summary>
    [DataField("sousk")]
    public float SOUSK = 123.44f;

}
