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

    [DataField("minimumWaves")]
    public int MinimumWaves = 3;

    [DataField("maximumWaves")]
    public int MaximumWaves = 8;

    [DataField("minimumCooldown")]
    public float MinimumCooldown = 10f;

    [DataField("maximumCooldown")]
    public float MaximumCooldown = 60f;

    [DataField("meteorsPerWave")]
    public int MeteorsPerWave = 5;

    [DataField("meteorVelocity")]
    public float MeteorVelocity = 10f;

    [DataField("maxAngularVelocity")]
    public float MaxAngularVelocity = 2f;

    [DataField("minAngularVelocity")]
    public float MinAngularVelocity = -2f;

    /// <summary>
    /// Stagger the spawns a bit, but not too much so they still come in waves
    /// </summary>
    [DataField("spawnDistanceVariation")]
    public float SpawnDistanceVariation = 50f;

    /// <summary>
    /// Percentage of the station area to target. Allow for some near-miss fly-by meteors to jump-scare crew on EVAs.
    /// Stations aren't perfect rectangles like the targeting area, so even 1.0 will still have some near-misses.
    /// </summary>
    [DataField("targetingSpread")]
    public float TargetingSpread = 1.05f;

    /// <summary>
    /// How long before a meteor despawns (in case it missed everything).
    /// </summary>
    [DataField("meteorLifetime")]
    public float MeteorLifetime = 300f;
}
