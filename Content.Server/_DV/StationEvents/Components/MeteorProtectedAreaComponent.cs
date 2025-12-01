using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

/// <summary>
/// DeltaV - Causes meteor swarms to avoid hitting near this component.
/// </summary>
[RegisterComponent]
public sealed partial class MeteorProtectedAreaComponent : Component
{
    [DataField]
    public bool Enabled = true;

    /// <summary>
    /// The size of the area around this entity to avoid.
    /// </summary>
    [DataField]
    public float ProtectionRadius = 5f;

    /// <summary>
    /// Percentage (roughly) of meteors that are BLOCKED from hitting near this component.
    /// </summary>
    [DataField]
    public float ProtectionRate = 0.75f;

    /// <summary>
    /// If true, Enabled will automatically be set to false if this entity is ever unanchored.
    /// </summary>
    [DataField]
    public bool DisablePermanentlyIfUnanchored = false;
}
