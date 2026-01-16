using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(MeteorSwarmRule))]
public sealed partial class MeteorSwarmRuleComponent : Component
{
    [DataField("initialCooldown")]
    public float Cooldown;

    public bool IsEnding = false;

    /// <summary>
    /// Send a specific amount of waves of meteors towards the station, rather than a random amount.
    /// </summary>
    [DataField("waves")]
    public int? WaveCounter = null;

    [DataField]
    public int MinimumWaves = 3;

    [DataField]
    public int MaximumWaves = 8;

    [DataField]
    public float MinimumCooldown = 10f;

    [DataField]
    public float MaximumCooldown = 60f;

    [DataField]
    public int MeteorsPerWave = 5;

    [DataField]
    public float MeteorVelocity = 10f;

    [DataField]
    public float MaxAngularVelocity = 2f;

    [DataField]
    public float MinAngularVelocity = -2f;

    /// <summary>
    /// Stagger the spawns a bit, but not too much so they still come in waves
    /// </summary>
    [DataField]
    public float SpawnDistanceVariation = 50f;

    /// <summary>
    /// Percentage of the station area to target. Allow for some near-miss fly-by meteors to jump-scare crew on EVAs.
    /// Stations aren't perfect rectangles like the targeting area, so even 1.0 will still have some near-misses.
    /// </summary>
    [DataField]
    public float TargetingSpread = 1.05f;

    /// <summary>
    /// How long before a meteor despawns (in case it missed everything).
    /// </summary>
    [DataField]
    public float MeteorLifetime = 300f;
}
