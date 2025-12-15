using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Server.Atmos.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Spawners;

namespace Content.Server.StationEvents.Events
{
    public sealed class MeteorSwarmRule : StationEventSystem<MeteorSwarmRuleComponent>
    {
        [Dependency] private readonly SharedMapSystem _map = default!;
        [Dependency] private readonly SharedPhysicsSystem _physics = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;


        protected override void Started(EntityUid uid, MeteorSwarmRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
        {
            base.Started(uid, component, gameRule, args);

            if (component.WaveCounter == null)
            {
                component.WaveCounter = RobustRandom.Next(component.MinimumWaves, component.MaximumWaves);
            }

            if (component.WaveCounter <= 0)
            {
                component.IsEnding = true;
                component.Cooldown = 0;
            }
        }

        protected override void ActiveTick(EntityUid uid, MeteorSwarmRuleComponent component, GameRuleComponent gameRule, float frameTime)
        {
            component.Cooldown -= frameTime;

            if (component.Cooldown > 0f)
                return;

            if (component.IsEnding)
            {
                ForceEndSelf(uid, gameRule);
                return;
            }

            if (!TrySpawnMeteorWave(component, out var wave))
            {
                ForceEndSelf(uid, gameRule);
                return;
            }

            component.WaveCounter--;
            if (component.WaveCounter <= 0)
            {
                component.IsEnding = true;

                // DeltaV space maps are quite large so it can take 1-2 minutes for the meteors to arrive.
                // Delay "meteor swarm finished" announcement until just after last meteor is scheduled to strike
                component.Cooldown += wave.TimeUntilLastImpact + RobustRandom.NextFloat(5f, 10f);
            }
            else
            {
                component.Cooldown += RobustRandom.NextFloat(component.MinimumCooldown, component.MaximumCooldown);
            }
        }


        private record MeteorWaveStatistics(
            float TimeUntilFirstImpact,
            float TimeUntilLastImpact
        );

        private bool TrySpawnMeteorWave(MeteorSwarmRuleComponent component, [NotNullWhen(true)] out MeteorWaveStatistics? waveStatistics)
        {
            var station = GetStation();

            // use a dud meteor if there's an atmosphere to "simulate" burning up
            var proto = "MeteorLargeDeltaV";
            if (_map.TryGetMap(station.FocalPoint.MapId, out var mapUid) && TryComp<MapAtmosphereComponent>(mapUid, out var atmos) && !atmos.Space)
                proto = "MeteorGlacierDeltaV";

            Box2? playableArea = null;
            var query = AllEntityQuery<MapGridComponent, TransformComponent>();
            while (query.MoveNext(out var gridId, out _, out var xform))
            {
                if (xform.MapID != station.FocalPoint.MapId)
                    continue;

                var aabb = _physics.GetWorldAABB(gridId);
                playableArea = playableArea?.Union(aabb) ?? aabb;
            }
            if (playableArea == null)
            {
                waveStatistics = null;
                return false;
            }

            // spawn meteors on the space map periphery, so they have a chance to hit any space objects, not just the station
            var spawnMinimumDistance = (playableArea.Value.TopRight - playableArea.Value.Center).Length() + 50f;
            var spawnMaximumDistance = spawnMinimumDistance + component.SpawnDistanceVariation;

            var stationTargetingSpread = (station.Area.TopRight - station.Area.Center).Length() * component.TargetingSpread;

            var antiMeteorZones = GetAntiMeteorZones();

            // If we haven't yet decided on how to coordinate the swarm (i.e. this is the first wave)
            // then decide now.
            if (component.BiasEnabled && component.BiasApproachAngleThisSwarm == null)
            {
                if (!component.BiasEnabledForTarget)
                {
                    // Only coordinate the angle that meteors approach from.
                    component.BiasApproachAngleThisSwarm = RobustRandom.NextAngle();
                    // Target that entire side of the station, rather than a specific point / outside-facing room.
                    component.BiasTargetThisSwarm = null;
                }
                else
                {
                    // Concentrate fire upon a random outside-facing room...

                    // Make sure we aren't targeting an anti-meteor zone
                    var protectedZonesThisSwarm =
                        antiMeteorZones.Where(zone => RobustRandom.Prob(zone.ProtectionRate)).ToList();

                    var stationWallTarget = SampleUntilAcceptable(
                        nextSample: () => {
                            var target = NextStationWallTarget(RobustRandom, station);

                            var exampleTrajectory = new MeteorTrajectory(
                                StartPosition: new MapCoordinates(
                                    // "rewind" the meteor's trajectory all the way to the edge of the space map
                                    target.Target.Position - (target.ApproachAngle.RotateVec(new Vector2(spawnMaximumDistance, 0))),
                                    target.Target.MapId
                                ),
                                Velocity: target.ApproachAngle.RotateVec(new Vector2(component.MeteorVelocity))
                            );

                            var acceptable = protectedZonesThisSwarm.All(zone => !TrajectoryIntersectsZone(exampleTrajectory, zone));

                            return (target, acceptable);
                        },
                        maxAttempts: 5
                    );

                    component.BiasTargetThisSwarm = stationWallTarget.Target;
                    component.BiasApproachAngleThisSwarm = stationWallTarget.ApproachAngle;
                }

                // makes it much easier to read in the View Variables window
                component.BiasApproachAngleThisSwarm = component.BiasApproachAngleThisSwarm?.Reduced();
            }


            float minImpactTime = 0, maxImpactTime = 0;
            for (var i = 0; i < component.MeteorsPerWave; i++)
            {
                var protectedZonesThisMeteor =
                    antiMeteorZones.Where(zone => RobustRandom.Prob(zone.ProtectionRate)).ToList();

                var thisMeteorBiased = component.BiasEnabled && RobustRandom.Prob(component.BiasRate);

                var trajectory = SampleUntilAcceptable(
                    nextSample: () => {
                        Angle approachAngleThisMeteor;
                        if (component.BiasApproachAngleThisSwarm is {} biasApproachAngle && thisMeteorBiased)
                        {
                            approachAngleThisMeteor = new Angle(RobustRandom.NextGaussian(
                                biasApproachAngle,
                                MathHelper.DegreesToRadians(component.BiasApproachAngleDeviationDegrees)
                            ));
                        }
                        else
                        {
                            approachAngleThisMeteor = RobustRandom.NextAngle();
                        }

                        MapCoordinates targetThisMeteor;
                        Vector2 targetingSpreadThisMeteor;
                        if (component.BiasTargetThisSwarm is {} biasTarget && thisMeteorBiased)
                        {
                            targetThisMeteor = biasTarget;
                            targetingSpreadThisMeteor =
                                RobustRandom.NextAngle().RotateVec(new Vector2((float)RobustRandom.NextGaussian(0, component.BiasTargetDeviationTiles), 0));
                        }
                        else
                        {
                            targetThisMeteor = station.FocalPoint;
                            targetingSpreadThisMeteor = new Vector2(
                                RobustRandom.NextFloat(-stationTargetingSpread, stationTargetingSpread),
                                RobustRandom.NextFloat(-stationTargetingSpread, stationTargetingSpread)
                            );
                        }

                        var approachAngleThisMeteorVectorized = approachAngleThisMeteor.RotateVec(new Vector2(1, 0));
                        // "rewind" the meteor's trajectory all the way to the edge of the space map
                        var spawnOffset = -(approachAngleThisMeteor.RotateVec(new Vector2(RobustRandom.NextFloat(spawnMinimumDistance, spawnMaximumDistance), 0)));

                        var trajectory_ = new MeteorTrajectory(
                            StartPosition: new MapCoordinates(
                                targetThisMeteor.Position + targetingSpreadThisMeteor + spawnOffset,
                                targetThisMeteor.MapId
                            ),
                            Velocity: approachAngleThisMeteor.RotateVec(new Vector2(component.MeteorVelocity, 0))
                        );

                        var acceptable = (
                            (component.BiasTargetThisSwarm != null && thisMeteorBiased)
                            ? true // we already did protected zone checking on the whole swarm's bias target
                            : protectedZonesThisMeteor.All(zone => !TrajectoryIntersectsZone(trajectory_, zone))
                        );

                        return (trajectory_, acceptable);
                    },
                    maxAttempts: 3
                );

                var meteor = Spawn(proto, trajectory.StartPosition);
                var physics = EntityManager.GetComponent<PhysicsComponent>(meteor);
                _physics.SetBodyStatus(meteor, physics, BodyStatus.InAir);
                _physics.SetLinearDamping(meteor, physics, 0f);
                _physics.SetAngularDamping(meteor, physics, 0f);
                _physics.ApplyLinearImpulse(meteor, trajectory.Velocity * physics.Mass, body: physics);
                _physics.SetAngularVelocity(meteor, RobustRandom.NextFloat(component.MinAngularVelocity, component.MaxAngularVelocity), body: physics);

                EnsureComp<TimedDespawnComponent>(meteor).Lifetime = component.MeteorLifetime;

                var estimatedTimeUntilImpact = TimeUntilClosestApproach(trajectory, station.FocalPoint);
                if (i == 0 || estimatedTimeUntilImpact < minImpactTime)
                {
                    minImpactTime = estimatedTimeUntilImpact;
                }
                if (i == 0 || estimatedTimeUntilImpact > maxImpactTime)
                {
                    maxImpactTime = estimatedTimeUntilImpact;
                }
            }

            waveStatistics = new MeteorWaveStatistics(
                TimeUntilFirstImpact: minImpactTime,
                TimeUntilLastImpact: maxImpactTime
            );
            return true;
        }


        private record Station (
            MapCoordinates FocalPoint,
            Box2 Area
        );

        // Get the space station... the 14th one.
        // (in theory though, we could target this event at other "stations" in the future -- ATS, Midpoint, Redrock... :D )
        private Station GetStation()
        {
            var focalPointOnStationEntity = GameTicker.GetObserverSpawnPoint();
            var focalPoint = _transform.ToMapCoordinates(focalPointOnStationEntity);

            var area = _physics.GetWorldAABB(focalPointOnStationEntity.EntityId);

            return new Station(focalPoint, area);
        }


        private record AntiMeteorZoneSummary (
            MapCoordinates Center,
            float RadiusSquared,
            float ProtectionRate
        );

        private List<AntiMeteorZoneSummary> GetAntiMeteorZones()
        {
            var zoneSummaries = new List<AntiMeteorZoneSummary>();

            var zoneQuery = AllEntityQuery<AntiMeteorZoneComponent, TransformComponent>();
            while (zoneQuery.MoveNext(out var protectedEntityUid, out var zone, out var transform))
            {
                zoneSummaries.Add(new AntiMeteorZoneSummary(
                    Center: _transform.ToMapCoordinates(transform.Coordinates),
                    RadiusSquared: MathF.Pow(zone.ZoneRadius, 2f),
                    ProtectionRate: zone.AvoidanceRate
                ));
            }

            // dang, I wish I could use Linq on entity queries (just Concat the above and below)
            var standardRateZoneQuery = AllEntityQuery<AntiMeteorZoneStandardRateComponent, TransformComponent>();
            while (standardRateZoneQuery.MoveNext(out var protectedEntityUid, out var zone, out var transform))
            {
                zoneSummaries.Add(new AntiMeteorZoneSummary(
                    Center: _transform.ToMapCoordinates(transform.Coordinates),
                    RadiusSquared: MathF.Pow(zone.ZoneRadius, 2f),
                    ProtectionRate: zone.AvoidanceRate
                ));
            }

            return zoneSummaries;
        }


        /// <summary>
        /// Selects a random target point on the station perimeter, and generates a reasonable approach angle for meteors.
        /// </summary>
        private static (MapCoordinates Target, Angle ApproachAngle) NextStationWallTarget(IRobustRandom robustRandom, Station station)
        {
            // First: choose which side of the station the target point will be on.
            var targetAngle = robustRandom.NextAngle();

            // Second: meteors approach *from* that side.
            var approachAngle = targetAngle + MathF.PI;
            // We don't have to approach that point head-on -- we can vary the approach a bit.
            //  (in fact, some station perimeters have complex geometry -- certain targets can only be hit from off-angle approaches)
            //  But don't vary *too* much -- approaching from the opposite side of the station would hit something else!
            approachAngle += robustRandom.NextAngle(-MathF.PI / 3f, MathF.PI / 3f);

            // Finally: focus trajectories onto a particular wall on that side of the station, so we space some rooms!
            // Extremely rough estimate: The station is a perfect circle. The station's outermost walls are (on average)
            //  halfway between the station's center and the station's outermost extremities (e.g. the solars / ai sat)
            var stationTotalRadius = (station.Area.TopRight - station.Area.Center).Length();
            var stationWallsRadius = stationTotalRadius * 0.5f;
            var target = new MapCoordinates(
                station.FocalPoint.Position + targetAngle.RotateVec(new Vector2(stationWallsRadius, 0)),
                station.FocalPoint.MapId
            );

            return (target, approachAngle);
        }


        /// <summary>
        /// Rejection sampling. Samples from the given generator until an acceptable sample is found.
        /// </summary>
        private static TSample SampleUntilAcceptable<TSample>(
            Func<(TSample sample, bool acceptable)> nextSample,
            int maxAttempts
        )
        {
            for (int i = 0; i < maxAttempts - 1; i++)
            {
                var sample = nextSample();
                if (sample.acceptable)
                {
                    return sample.sample;
                }
            }

            // this is our last attempt, just return even if unacceptable
            return nextSample().sample;
        }


        private record MeteorTrajectory (
            MapCoordinates StartPosition,
            Vector2 Velocity
        );

        /// <summary>
        /// The amount of time until an object on a constant linear trajectory will "pass" a specific target point.
        /// The object may not necessarily intersect/impact that target point -- this "closest approach" may be at a distance.
        /// </summary>
        private static float TimeUntilClosestApproach(MeteorTrajectory trajectory, MapCoordinates approachPoint)
        {
            // Mathematical derivation:
            // The distance between approachPoint (a) and objectPosition (p)
            //  = Distance between origin and relativeObjectPosition (p0)
            //  = Distance between origin and (p0 + velocity * t)
            //  = Magnitude of (p0 + v*t)
            //
            // We want the `t` that minimizes that magnitude. Calculus says: find the critical points.
            // (I'm lazy; let wolfram alpha do the calculus for us)
            // 1. derivative of magnitude:  https://www.wolframalpha.com/input?i=given+m+%3D+%28x_0+%2B+vt%29%5E2+%2B+%28y_0+%2B+ut%29%5E2+find+dm%2Fdt
            // 2. set derivative=0 and solve for t:  https://www.wolframalpha.com/input?i=2v%28x_0+%2B+vt%29+%2B+2u%28y_0+%2B+ut%29+%3D+0+solve+for+t
            // Result:       -(vx*x0 + vy*y0)
            //         t = --------------------
            //                 (vx^2 + vy^2)

            var relativeStartPoint = new Vector2(trajectory.StartPosition.X - approachPoint.X, trajectory.StartPosition.Y - approachPoint.Y);
            return -Vector2.Dot(trajectory.Velocity, relativeStartPoint) / trajectory.Velocity.LengthSquared();
        }

        /// <summary>
        /// Returns true if an object on the given constant linear trajectory will pass through the given zone.
        /// </summary>
        /// <remarks>
        /// This is basically a raycast, but without checking for other obstacles that are "in the way" like the actual Raycast() does.
        /// i.e. Even if this returns true, an obstacle might "block" the object on its path towards the area.
        /// </remarks>
        private static bool TrajectoryIntersectsZone(MeteorTrajectory trajectory, AntiMeteorZoneSummary zone)
        {
            if (trajectory.StartPosition.MapId != zone.Center.MapId)
                return false;

            var timeUntilClosestApproachToZone = TimeUntilClosestApproach(trajectory, zone.Center);
            var pointOfClosestApproachToZone = trajectory.StartPosition.Position + trajectory.Velocity * timeUntilClosestApproachToZone;
            return (
                timeUntilClosestApproachToZone >= 0
                && (
                    Vector2.DistanceSquared(zone.Center.Position, pointOfClosestApproachToZone)
                    < zone.RadiusSquared
                )
            );
        }
    }
}
