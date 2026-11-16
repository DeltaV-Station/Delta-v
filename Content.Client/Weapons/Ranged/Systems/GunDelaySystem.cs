using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared._DV.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Client._DV.Weapons.Ranged.Systems;

public sealed class GunDelaySystem : SharedGunDelaySystem
{
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    protected override void OnShotAttempted(Entity<GunDelayComponent> ent, ref ShotAttemptedEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.Lit)
        {
            if (ent.Comp.Delay != 0) // Don't need to play the start sound for the delay if there isn't one
                AudioSystem.PlayPvs(ent.Comp.StartSound, ent);

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
                Dirty(uid, comp);
                continue;
            }

            comp.Firing = true; // Firing should be set to true *only* when the gun is actively firing as to not block shot
            Dirty(uid, comp);

            // AttemptShoot shouldn't be run by the client *unless* the client's character is the one shooting
            // Basically if you're the one who is holding the gun that is firing you actually SHOULD run AttemptShoot so this is just stuff for that
            if (gun.Holder == _player.LocalEntity)
                GunSystem.AttemptShoot(user, (uid, gun), new EntityCoordinates(uid, dir), gun.Target);
        }
    }
}