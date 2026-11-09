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

public sealed class ChanceGunSystem : EntitySystem
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

        SubscribeLocalEvent<ChanceGunComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<ChanceGunComponent, DroppedEvent>(OnDropped);
    }

    private void OnDropped(Entity<ChanceGunComponent> ent, ref DroppedEvent args)
    {
        ent.Comp.User = ent;
        Dirty(ent);
    }

    private void OnShotAttempted(Entity<ChanceGunComponent> ent, ref ShotAttemptedEvent args)
    {
        if (!ent.Comp.Fired)
        {
            ent.Comp.FireTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(3.0f, 5.0f));
            _audio.PlayPredicted(ent.Comp.StartSound, ent, args.User);
            if (_net.IsServer)
                ent.Comp.PlayingSound = _audio.PlayPvs(ent.Comp.LoopSound, ent, AudioParams.Default.WithLoop(true))?.Entity;
            ent.Comp.Fired = true;
            ent.Comp.User = args.User;
            Dirty(ent);
        }

        if (ent.Comp.Broken || !ent.Comp.Lit)
        {
            args.Cancel();
            return;
        }

    }
    private void CheckShot()
    {
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ChanceGunComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Broken && comp.Fired)
            {
                if (comp.FireTime > curTime)
                    return;

                if (!TryComp(uid, out GunComponent? gun))
                    return;

                if (comp.User is not { } user)
                    return;

                var dir = gun.DefaultDirection;
                _audio.Stop(comp.PlayingSound);

                if (user == uid)
                {
                    if (_net.IsClient)
                        return;
                    var throwDirection = _transform.GetWorldRotation(uid).ToWorldVec();
                    throwDirection = new Vector2(throwDirection.Y, -throwDirection.X) * 600;
                    dir = new Vector2(-dir.Y, dir.X);
                    _physics.ApplyLinearImpulse(uid, throwDirection);
                    comp.Lit = true;
                    comp.Broken = _gunSystem.AttemptShoot(uid, (uid, gun), new EntityCoordinates(uid, dir));
                    Dirty(uid, comp);
                    return;
                }

                comp.Lit = true;
                comp.Broken = _gunSystem.AttemptShoot(user, (uid, gun), new EntityCoordinates(uid, dir), gun.Target);
                Dirty(uid, comp);
            }
        }
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        CheckShot();
    }
}
