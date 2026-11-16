using System.Numerics;
using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared._DV.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio;
using Robust.Shared.Map;


namespace Content.Server._DV.Weapons.Ranged.Systems;

public sealed class GunDelaySystem : SharedGunDelaySystem
{
    protected override void OnShotAttempted(Entity<GunDelayComponent> ent, ref ShotAttemptedEvent args)
    {
        if (args.Cancelled) // Has this already been cancled?
            return;

        if (!ent.Comp.Firing) // Have we not fired, setting fire time yet?
        {
            GunSystem.AmmoDelay(ent, ref args); // Checks if the ammo wants to set delay
            if (ent.Comp.Delay is { } delay) // Checks if we should use random delay or flat delay
                ent.Comp.FireTime = Timing.CurTime + TimeSpan.FromSeconds(delay);
            else
                ent.Comp.FireTime = Timing.CurTime + TimeSpan.FromSeconds(Random.NextFloat(ent.Comp.MinimumDelay, ent.Comp.MaximumDelay));

            if (ent.Comp.Delay != 0) // Don't need to play the start sound for the delay if there isn't one
                AudioSystem.PlayPredicted(ent.Comp.StartSound, ent, args.User);

            ent.Comp.PlayingSound = AudioSystem.PlayPvs(ent.Comp.LoopSound, ent, AudioParams.Default.WithLoop(true))?.Entity;
            ent.Comp.Firing = true;
            ent.Comp.User = args.User;
            Dirty(ent);
        }

        if (!ent.Comp.Lit) // Is the gun actually supposed to fire?
            args.Cancel();
    }

    protected override void CheckShot()
    {
        var curTime = Timing.CurTime;
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
            AudioSystem.Stop(comp.PlayingSound);

            if (gun.Holder is not { }) // Is the gun not being held?
            {
                comp.Lit = true; // Lit should be set to true *only* when the gun is actively firing as to not block shot
                dir = new Vector2(-dir.Y, dir.X);
                GunSystem.AttemptShoot(uid, (uid, gun), new EntityCoordinates(uid, dir));
                Dirty(uid, comp);
                continue;
            }

            comp.Lit = true; // Lit should be set to true *only* when the gun is actively firing as to not block shot

            GunSystem.AttemptShoot(user, (uid, gun), new EntityCoordinates(uid, dir), gun.Target);
            Dirty(uid, comp);
        }
    }
}

