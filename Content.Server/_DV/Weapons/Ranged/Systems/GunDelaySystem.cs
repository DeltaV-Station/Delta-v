using System.Numerics;
using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared._DV.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Random;


namespace Content.Server._DV.Weapons.Ranged.Systems;

public sealed class GunDelaySystem : SharedGunDelaySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    protected override void OnShotAttempted(Entity<GunDelayComponent> ent, ref ShotAttemptedEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.Lit)
        {
            GunSystem.AmmoDelay(ent, ref args); // Checks if the ammo wants to set delay
            if (ent.Comp.Delay is { } delay) // Use flat delay if one is provided
                ent.Comp.FireTime = Timing.CurTime + TimeSpan.FromSeconds(delay);
            else
                ent.Comp.FireTime = Timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(ent.Comp.MinimumDelay, ent.Comp.MaximumDelay));

            if (ent.Comp.Delay != 0) // Don't need to play the start sound for the delay if there isn't one
                AudioSystem.PlayPredicted(ent.Comp.StartSound, ent, args.User);

            ent.Comp.PlayingSound = AudioSystem.PlayPvs(ent.Comp.LoopSound, ent, AudioParams.Default.WithLoop(true))?.Entity;
            ent.Comp.Lit = true;
            ent.Comp.User = args.User;
            Dirty(ent);
        }

        if (!ent.Comp.Firing) // Because of the fact that the gun firing will trigger this function, we need to block it unless it is firing
            args.Cancel();
    }

    protected override void CheckShot()
    {
        var curTime = Timing.CurTime;
        var query = EntityQueryEnumerator<GunDelayComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Lit)
                continue;

            if (comp.FireTime > curTime)
                continue;

            if (!TryComp(uid, out GunComponent? gun))
                continue;

            if (comp.User is not { } user)
                continue;
            var dir = gun.DefaultDirection;
            AudioSystem.Stop(comp.PlayingSound);

            if (gun.Holder is not { })
            {
                comp.Firing = true; // Firing should be set to true *only* when the gun is actively firing as to not block shot
                dir = new Vector2(-dir.Y, dir.X); // Need to invert the vector so the gun shoots in the right direction instead of straight down
                GunSystem.AttemptShoot(uid, (uid, gun), new EntityCoordinates(uid, dir));
                Dirty(uid, comp);
                continue;
            }

            comp.Firing = true; // Firing should be set to true *only* when the gun is actively firing as to not block shot

            GunSystem.AttemptShoot(user, (uid, gun), new EntityCoordinates(uid, dir), gun.Target);
            Dirty(uid, comp);
        }
    }
}
