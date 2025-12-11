using System.Numerics;
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
    public float SpawnDistanceVariation = 75f;

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

    /// <summary>
    /// If true, a BiasRate proportion of meteors will be aligned to a randomly chosen approach angle, 
    /// and (if TargetBiasEnabled) coordinated to a specific target impact point on the station.
    /// </summary>
    [DataField]
    public bool BiasEnabled = false;

    /// <summary>
    /// Whether biasing should ALSO target meteors at a specific target impact point on the perimeter of the station.
    /// </summary>
    /// <remarks>
    /// - BiasEnabled false                         = approach and target are both random
    /// - BiasEnabled true, TargetBiasEnabled false = approach coordinated, but target random
    /// - BiasEnabled true, TargetBiasEnabled true  = approach coordinated, target coordinated
    /// </remarks>
    [DataField]
    public bool TargetBiasEnabled = false;

    /// <summary>
    /// If BiasEnabled, proportion of meteors to bias towards a randomly-chosen swarm approach angle and target impact point.
    /// </summary>
    [DataField]
    public float BiasRate = 0.8f;

    /// <summary>
    /// Standard deviation of biased meteors' approach angle - meteors will follow a normal distribution around a 
    /// randomly-chosen coordinated swarm angle.
    /// </summary>
    /// <remarks>
    /// e.g. by the 68-95-99 rule, 68% of biased meteors will approach the station at an angle 
    /// within this many radians of the randomly-chosen coordinated swarm angle.
    /// 
    /// Note: if this deviation >= 1.0, the "default to uniform distribution" feature of the sampling will start to 
    /// noticeably affect randomness, artifically raising the BiasRate beyond the specified value.
    /// See the statistical implementation of MeteorSwarmRule.NextBiasedConstrainedFloat() for more context.
    /// </remarks>
    [DataField]
    public float ApproachBiasDeviation = 0.6f;

    /// <summary>
    /// Standard deviation of biased meteors' target impact point - meteors will target a normal distribution 
    /// around a coordinated swarm target impact point randomly-chosen from the station's perimeter.
    /// </summary>
    /// <remarks>
    /// e.g. by the 68-95-99 rule, 68% of biased meteors will impact the station within
    /// this many tiles of the randomly-chosen coordinated swarm impact point.
    /// </remarks>
    [DataField]
    public float TargetBiasDeviation = 5f;

    [DataField]
    public (Vector2 target, Angle approachAngle)? SelectedBias = null;
}
