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
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Content.Shared.Hands;
using Robust.Shared.Player;

namespace Content.Shared._DV.Weapons.Ranged.Systems;

public sealed class GunDelaySystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ActorSystem _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunDelayComponent, ShotAttemptedEvent>(OnShotAttempted, after: [typeof(GunBreakingSystem)]);
        SubscribeLocalEvent<GunDelayComponent, DroppedEvent>(OnDropped);
        SubscribeLocalEvent<GunDelayComponent, GunShotEvent>(OnShot);
        SubscribeLocalEvent<GunDelayComponent, GotEquippedHandEvent>(OnPickup);
    }

    private void OnDropped(Entity<GunDelayComponent> ent, ref DroppedEvent args)
    {
        ent.Comp.User = ent;
        Dirty(ent);
    }

    private void OnPickup(Entity<GunDelayComponent> ent, ref GotEquippedHandEvent args)
    {
        ent.Comp.User = args.User;
        Dirty(ent);
    }

    private void OnShotAttempted(Entity<GunDelayComponent> ent, ref ShotAttemptedEvent args)
    {
        if (args.Cancelled) // Has this already been cancled?
            return;

        if (!ent.Comp.Firing) // Have we not fired, setting fire time yet?
        {
            _gunSystem.AmmoDelay(ent, ref args); // Checks if the ammo wants to set delay
            if (ent.Comp.Delay is { } delay) // Checks if we should use random delay or flat delay
                ent.Comp.FireTime = _timing.CurTime + TimeSpan.FromSeconds(delay);
            else
                ent.Comp.FireTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(ent.Comp.MinimumDelay, ent.Comp.MaximumDelay));

            if (ent.Comp.Delay != 0) // Don't need to play the start sound for the delay if there isn't one
                _audio.PlayPredicted(ent.Comp.StartSound, ent, args.User);

            if (_net.IsServer)
                ent.Comp.PlayingSound = _audio.PlayPvs(ent.Comp.LoopSound, ent, AudioParams.Default.WithLoop(true))?.Entity;
            ent.Comp.Firing = true;
            ent.Comp.User = args.User;
            Dirty(ent);
        }

        if (!ent.Comp.Lit) // Is the gun actually supposed to fire?
            args.Cancel();
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

            if (gun.Holder is not { }) // Is the gun not being held?
            {
                comp.Lit = true; // Lit should be set to true *only* when the gun is actively firing as to not block shot
                if (_net.IsServer) // Client should not be running AttemptShoot*
                {
                    dir = new Vector2(-dir.Y, dir.X);
                    _gunSystem.AttemptShoot(uid, (uid, gun), new EntityCoordinates(uid, dir));
                }
                Dirty(uid, comp);
                continue;
            }

            comp.Lit = true; // Lit should be set to true *only* when the gun is actively firing as to not block shot

            // Hey, remember that asterisk up there? Well here's where that comes into play,
            // AttemptShoot shouldn't be run by the client *unless* the "Client" is the one shooting
            // Basically if you're the one who is holding the gun that is firing you actually SHOULD run AttemptShoot so this is just stuff for that
            if (_net.IsClient)
            {
                _player.TryGetSession(gun.Holder, out var session);
                if (session is not { })
                    continue;
                if (session.AttachedEntity == gun.Holder)
                    _gunSystem.AttemptShoot(user, (uid, gun), new EntityCoordinates(uid, dir), gun.Target);
            }
            else // Server doesn't need to check all of that
                _gunSystem.AttemptShoot(user, (uid, gun), new EntityCoordinates(uid, dir), gun.Target);
            Dirty(uid, comp);
        }
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        CheckShot();
    }
    private void OnShot(Entity<GunDelayComponent> ent, ref GunShotEvent args) // Handles the resetting after the shot is fired
    {
        ent.Comp.Lit = false;
        ent.Comp.Firing = false;
        Dirty(ent);
    }
}
