using System.Numerics;
using Content.Server.Atmos.Components;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Spawners;

namespace Content.Server.StationEvents.Events
{
    public sealed class MeteorSwarmRule : StationEventSystem<MeteorSwarmRuleComponent>
    {
        [Dependency] private readonly SharedMapSystem _map = default!;
        [Dependency] private readonly SharedPhysicsSystem _physics = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;


        private (MapCoordinates target, Box2 targetArea) GetTarget() 
        {
            var targetOnEntity = GameTicker.GetObserverSpawnPoint();
            var target = _transform.ToMapCoordinates(targetOnEntity);

            // target various points on the station
            var targetArea = _physics.GetWorldAABB(targetOnEntity.EntityId);

            return (target, targetArea);
        }

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
            else 
            {
                (var target, var targetArea) = GetTarget();
                _chatManager.SendAdminAlert(Loc.GetString(
                    "station-event-system-meteor-swarm-starting",
                    ("count", component.WaveCounter),
                    ("targetCorner1", $"{targetArea.TopLeft.X:F0},{targetArea.TopLeft.Y:F0}"),
                    ("targetCorner2", $"{targetArea.BottomRight.X:F0},{targetArea.BottomRight.Y:F0}")
                ));
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

            var mapId = GameTicker.DefaultMap;
            // use a dud meteor if there's an atmosphere to "simulate" burning up
            var proto = "MeteorLargeDeltaV";
            if (_map.TryGetMap(mapId, out var mapUid) && TryComp<MapAtmosphereComponent>(mapUid, out var atmos) && !atmos.Space)
                proto = "MeteorGlacierDeltaV";

            Box2? playableArea = null;
            var query = AllEntityQuery<MapGridComponent, TransformComponent>();
            while (query.MoveNext(out var gridId, out _, out var xform))
            {
                if (xform.MapID != mapId)
                    continue;

                var aabb = _physics.GetWorldAABB(gridId);
                playableArea = playableArea?.Union(aabb) ?? aabb;
            }
            if (playableArea == null)
            {
                ForceEndSelf(uid, gameRule);
                return;
            }

            // spawn meteors on the space map periphery, so they have a chance to hit any space objects, not just the station
            var minimumDistance = (playableArea.Value.TopRight - playableArea.Value.Center).Length() + 50f;
            var maximumDistance = minimumDistance + component.SpawnDistanceVariation;

            (var target, var targetArea) = GetTarget();
            var targetSpread = (targetArea.TopRight - targetArea.Center).Length() * component.TargetingSpread; 

            float minImpactTime = 0;
            float maxImpactTime = 0;
            for (var i = 0; i < component.MeteorsPerWave; i++)
            {
                var angle = new Angle(RobustRandom.NextFloat() * MathF.Tau);
                var offset = angle.RotateVec(new Vector2((maximumDistance - minimumDistance) * RobustRandom.NextFloat() + minimumDistance, 0));
                var spawnPosition = new MapCoordinates(
                    target.X + targetSpread * (2f * RobustRandom.NextFloat() - 1f) + offset.X, 
                    target.Y + targetSpread * (2f * RobustRandom.NextFloat() - 1f) + offset.Y,
                    mapId
                );
                var meteor = Spawn(proto, spawnPosition);
                var physics = EntityManager.GetComponent<PhysicsComponent>(meteor);
                _physics.SetBodyStatus(meteor, physics, BodyStatus.InAir);
                _physics.SetLinearDamping(meteor, physics, 0f);
                _physics.SetAngularDamping(meteor, physics, 0f);
                var velocity = -offset.Normalized() * component.MeteorVelocity;
                _physics.ApplyLinearImpulse(meteor, velocity * physics.Mass, body: physics);
                _physics.SetAngularVelocity(meteor, (component.MaxAngularVelocity - component.MinAngularVelocity) * RobustRandom.NextFloat() + component.MinAngularVelocity, body: physics);

                EnsureComp<TimedDespawnComponent>(meteor).Lifetime = component.MeteorLifetime;

                var relativeSpawnPosition = new Vector2(target.X - spawnPosition.X, target.Y - spawnPosition.Y);
                var timeUntilClosestApproach = (
                    (relativeSpawnPosition.X * velocity.X + relativeSpawnPosition.Y * velocity.Y) /
                    (MathF.Pow(velocity.X, 2f) + MathF.Pow(velocity.Y, 2f))
                );
                if (i == 0 || timeUntilClosestApproach < minImpactTime) 
                {
                    minImpactTime = timeUntilClosestApproach;
                }
                if (i == 0 || timeUntilClosestApproach > maxImpactTime)
                {
                    maxImpactTime = timeUntilClosestApproach;
                }
            }

            _chatManager.SendAdminAlert(Loc.GetString(
                "station-event-system-meteors-spawned",
                ("count", component.MeteorsPerWave),
                ("impactSeconds", $"{minImpactTime:F0}")
            ));

            component.WaveCounter--;
            if (component.WaveCounter <= 0) 
            {
                // DeltaV space maps are quite large so it can take 1-2 minutes for the meteors to arrive.
                // Delay "meteor swarm finished" announcement until just after last meteor is scheduled to strike
                component.IsEnding = true;
                component.Cooldown += maxImpactTime + (5f * RobustRandom.NextFloat()) + 5f;
            }
            else
            {
                component.Cooldown += (component.MaximumCooldown - component.MinimumCooldown) * RobustRandom.NextFloat() + component.MinimumCooldown;
            }
        }
    }
}
