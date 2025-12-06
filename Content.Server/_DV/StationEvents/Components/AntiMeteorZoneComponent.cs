using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

/// <summary>
/// DeltaV - Prohibits meteor swarms from targeting their impact near this component.
/// </summary>
[RegisterComponent]
public sealed partial class AntiMeteorZoneComponent : Component
{
    /// <summary>
    /// The size of the area around this entity to avoid.
    /// </summary>
    [DataField]
    public float ZoneRadius = 5f;

    /// <summary>
    /// Percentage (roughly) of meteors that are BLOCKED from hitting near this component.
    /// </summary>
    [DataField]
    public float AvoidanceRate = 0.75f;
}
