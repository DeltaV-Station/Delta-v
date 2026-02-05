using System.Numerics;
using Content.Shared._DV.TrafficHazard;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

public sealed partial class TrafficHazardSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = AllEntityQuery<TrafficHazardComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            // We were just hit by another traffic hazard, avoid re-fire
            if (_timing.CurTime < comp.AvoidRefire)
                continue;
            // Check each Traffic Hazard that is over their Minimum speed (Relative to their parent)
            if (!TryComp<PhysicsComponent>(uid, out var phys))
                continue;
            // Below speed.
            if (phys.LinearVelocity.LengthSquared() < comp.MinimumSpeed * comp.MinimumSpeed)
                continue;

            // Find all victims we may be overlapping
            var colliding = _physics.GetEntitiesIntersectingBody(uid, (int) CollisionGroup.MobLayer, true);
            foreach (var victim in colliding)
            {
                if (!TryComp<PhysicsComponent>(victim, out var otherPhys))
                    continue;
                if (!TryComp<DamageableComponent>(victim, out var otherDamage))
                    continue;
                CrawlerComponent? crawler = null;
                if (comp.StunTime > 0f){
                    // If it can never be knocked, don't hit it.
                    if (!TryComp(victim, out crawler))
                        continue;
                    // Ignore stunned entities to avoid repeat damage.
                    // (You can still runover down'd entities)
                    if (HasComp<StunnedComponent>(victim))
                        continue;
                }
                // Ensure our relative velocies are greater than threshold.
                // (If we're going opposite directions (Towards eachother), (other.vel - phys.vel) is larger than if we're going in the same direction (trying to outrun it))
                if ((otherPhys.LinearVelocity - phys.LinearVelocity).LengthSquared() < comp.MinimumSpeedDifference * comp.MinimumSpeedDifference)
                    continue;
                // If the other object is also a traffic hazard, give precedence to the faster moving hazard/
                if (TryComp<TrafficHazardComponent>(victim, out var otherComp))
                {
                    if (otherPhys.LinearVelocity.LengthSquared() < phys.LinearVelocity.LengthSquared())
                        otherComp.AvoidRefire = _timing.CurTime + TimeSpan.FromSeconds(1f);
                    else
                        continue;
                }
                if (comp.StunTime > 0f)
                    _stun.TryKnockdown(new(victim, crawler), TimeSpan.FromSeconds(comp.StunTime), true, true);
                // If StunTime is 0, we don't want to stun and work anyway.
                if (comp.StunTime <= 0f || _stun.TryAddStunDuration(victim, TimeSpan.FromSeconds(comp.StunTime)))
                {
                    // If we do not share a parent, we can't hit eachother.
                    // Not _always_ true, but true enough.
                    var ourXform = Transform(uid);
                    var otherXform = Transform(victim);
                    if (ourXform.ParentUid != otherXform.ParentUid)
                        continue;

                    if (comp.Bonk)
                    {
                        _throwing.TryThrow(victim, direction: phys.LinearVelocity, baseThrowSpeed: phys.LinearVelocity.Length() * 2.5f, uid, unanchor: ThrowingUnanchorStrength.None);
                        _physics.SetLinearVelocity(uid, Vector2.Zero);
                    }

                    // Deal damage on hit.
                    if (comp.CollisionDamage is { } damage)
                        _damage.TryChangeDamage(victim, damage);
                    // Play the collision sound if applicable
                    var coords = Transform(victim).Coordinates;
                    if (comp.CollisionSound is { } sound && _net.IsServer)
                        _audio.PlayPvs(sound, coords);
                }
            }
        }
    }
}
