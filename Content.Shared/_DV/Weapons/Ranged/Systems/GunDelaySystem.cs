using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Map;
using Content.Shared.Interaction.Events;
using System.Numerics;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;

namespace Content.Shared._DV.Weapons.Ranged.Systems;

public sealed class GunDelaySystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunDelayComponent, ShotAttemptedEvent>(OnShotAttempted, after: [typeof(GunBreakingSystem)]);
        SubscribeLocalEvent<GunDelayComponent, DroppedEvent>(OnDropped);
        SubscribeLocalEvent<GunDelayComponent, GunShotEvent>(OnShot);
    }

    private void OnDropped(Entity<GunDelayComponent> ent, ref DroppedEvent args)
    {
        ent.Comp.User = ent;
        Dirty(ent);
    }

    private void OnShotAttempted(Entity<GunDelayComponent> ent, ref ShotAttemptedEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.Firing)
        {
            ent.Comp.FireTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(3.0f, 5.0f));
            _audio.PlayPredicted(ent.Comp.StartSound, ent, args.User);
            if (_net.IsServer)
                ent.Comp.PlayingSound = _audio.PlayPvs(ent.Comp.LoopSound, ent, AudioParams.Default.WithLoop(true))?.Entity;
            ent.Comp.Firing = true;
            ent.Comp.User = args.User;
            Dirty(ent);
        }

        if (!ent.Comp.Lit)
        {
            args.Cancel();
            return;
        }

    }

    private void CheckShot()
    {
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<GunDelayComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Firing)
                continue;

            if (comp.FireTime > curTime)
                continue;

            if (!TryComp(uid, out GunComponent? gun))
                continue;

            if (comp.User is not { } user)
                continue;

            var dir = gun.DefaultDirection;
            _audio.Stop(comp.PlayingSound);

            if (user == uid)
            {
                if (_net.IsClient)
                    continue;
                dir = new Vector2(-dir.Y, dir.X);
                LaunchGun(uid);
                comp.Lit = true;
                _gunSystem.AttemptShoot(uid, (uid, gun), new EntityCoordinates(uid, dir));
                comp.Firing = false;
                Dirty(uid, comp);
                continue;
            }

            comp.Lit = true;
            _gunSystem.AttemptShoot(user, (uid, gun), new EntityCoordinates(uid, dir), gun.Target);
            comp.Firing = false;
            Dirty(uid, comp);
        }
    }
    private void LaunchGun(EntityUid gun)
    {
        var throwDirection = _transform.GetWorldRotation(gun).ToWorldVec();
        throwDirection = new Vector2(throwDirection.Y, -throwDirection.X) * 600;
        _physics.ApplyLinearImpulse(gun, throwDirection);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        CheckShot();
    }
    private void OnShot(Entity<GunDelayComponent> ent, ref GunShotEvent args)
    {
        ent.Comp.Lit = false;
        Dirty(ent);
    }
}
