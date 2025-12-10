using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

/// <summary>
/// DeltaV - Prohibits meteor swarms from targeting their impact near this component.
/// Standardized avoidance rate; cannot be changed in View Variables.
/// </summary>
[RegisterComponent]
public sealed partial class AntiMeteorZoneStandardRateComponent : Component
{
    /// <summary>
    /// The size of the area around this entity to avoid.
    /// </summary>
    [DataField]
    public float ZoneRadius = 5f;

    /// <summary>
    /// Percentage (roughly) of meteors that are BLOCKED from hitting near this component.
    /// Standardized avoidance rate; cannot be changed in View Variables.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)] // I tried to inherit AntiMeteorZoneComponent, but this VVAccess wouldn't take effect
    public float AvoidanceRate = 0.75f;
}
