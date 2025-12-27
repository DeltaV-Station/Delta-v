using System.Numerics;
using Content.Server.StationEvents.Events;
using Robust.Shared.Map;

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
    /// Whether to coordinate the meteor trajectories in this swarm.
    /// </summary>
    [DataField]
    public bool BiasEnabled = false;

    /// <summary>
    /// Proportion of meteors that will roughly follow the coordinated swarm trajectory.
    /// The remaining meteors will have totally-random trajectories.
    /// </summary>
    [DataField]
    public float BiasRate = 0.8f;

    /// <summary>
    /// Initialized automatically during the event -- a randomly-selected "swarm angle" that meteors will follow
    /// </summary>
    [DataField]
    public Angle? BiasApproachAngleThisSwarm = null;

    /// <summary>
    /// The level of coordination in the approach angle of the meteors.
    ///  (lower value = more aligned meteors)
    ///  (high value = meteors approaching from all angles)
    ///
    /// Details:
    /// An overall "swarm angle" will be randomly-chosen.
    /// Then, 68% * BiasRate% of meteors in the swarm will approach the station at an angle that's
    ///  within this many degrees of that "swarm angle".
    ///
    /// (search google for the "68-95-99 rule" of statistics)
    /// </summary>
    [DataField]
    public float BiasApproachAngleDeviationDegrees = 0.6f;

    /// <summary>
    /// Whether to ALSO coordinate the meteor target impact points in this swarm.
    /// </summary>
    /// <remarks>
    /// * BiasEnabled false                             =  approach angle and impact point are both completely random for each meteor.
    /// * BiasEnabled true, BiasEnabledForTarget false  =  approach angle coordinated, but each meteor has a random offset.
    /// * BiasEnabled true, BiasEnabledForTarget true   =  approach angle coordinated, target impact point coordinated.
    /// </remarks>
    [DataField]
    public bool BiasEnabledForTarget = false;

    /// <summary>
    /// Initialized automatically during the event -- a randomly-selected "swarm target point"
    /// </summary>
    [DataField]
    public MapCoordinates? BiasTargetThisSwarm = null;

    /// <summary>
    /// The level of coordination in the impact points of the meteors.
    ///  (lower value = more concentrated impacts)
    ///  (high value = impacts everywhere on the station perimeter)
    ///
    /// Details:
    /// An overall "swarm target point" will be randomly-chosen on the station's perimeter.
    /// Then, 68% * BiasRate% of meteors in the swarm will aim at a point
    ///  within this many tiles of that "swarm target point".
    ///
    /// (search google for the "68-95-99 rule" of statistics)
    /// </summary>
    [DataField]
    public float BiasTargetDeviationTiles = 5f;
}
