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

namespace Content.Shared._DV.Weapons.Ranged.Systems;

public sealed class GunDelaySystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

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
        if (args.Cancelled)
            return;

        if (!ent.Comp.Firing)
        {
            AmmoDelay(ent, ref args);
            if (ent.Comp.Delay is { } delay)
                ent.Comp.FireTime = _timing.CurTime + TimeSpan.FromSeconds(delay);
            else
                ent.Comp.FireTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(ent.Comp.MinimumDelay, ent.Comp.MaximumDelay));

            if (ent.Comp.Delay != 0)
                _audio.PlayPredicted(ent.Comp.StartSound, ent, args.User);

            if (_net.IsServer)
                ent.Comp.PlayingSound = _audio.PlayPvs(ent.Comp.LoopSound, ent, AudioParams.Default.WithLoop(true))?.Entity;
            ent.Comp.Firing = true;
            ent.Comp.User = args.User;
            Dirty(ent);
        }

        if (!ent.Comp.Lit)
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

            if (user == uid)
            {
                if (_net.IsClient)
                    continue;
                dir = new Vector2(-dir.Y, dir.X);
                comp.Lit = true;
                _gunSystem.AttemptShoot(uid, (uid, gun), new EntityCoordinates(uid, dir));
                Dirty(uid, comp);
                continue;
            }

            comp.Lit = true;
            _gunSystem.AttemptShoot(user, (uid, gun), new EntityCoordinates(uid, dir), gun.Target);
            Dirty(uid, comp);
        }
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        CheckShot();
    }
    private void OnShot(Entity<GunDelayComponent> ent, ref GunShotEvent args)
    {
        ent.Comp.Lit = false;
        ent.Comp.Firing = false;
        Dirty(ent);
    }
    private void AmmoDelay(Entity<GunDelayComponent> ent, ref ShotAttemptedEvent args)
    {
        if (_gunSystem.GetChamberEntity(ent) is not { } ammo)
            return;

        if (!TryComp(ammo, out AmmoDelayComponent? ammoDelay))
            return;

        if (ammoDelay.Delay is { })
        {
            ent.Comp.Delay = ammoDelay.Delay;
            return;
        }
        ent.Comp.MinimumDelay = ammoDelay.MinimumDelay;
        ent.Comp.MaximumDelay = ammoDelay.MaximumDelay;
        Dirty(args.Used.Owner, ent.Comp);
    }
}
