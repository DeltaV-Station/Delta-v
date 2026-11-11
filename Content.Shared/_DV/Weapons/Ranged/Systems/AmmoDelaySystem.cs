/*
using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;


namespace Content.Shared._DV.Weapons.Ranged.Systems;

public sealed class AmmoDelaySystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunDelayComponent, ShotAttemptedEvent>(OnShotAttempted); // Should probably get called directly on the ammo but whatever
    }
    private void OnShotAttempted(Entity<GunDelayComponent> ent, ref ShotAttemptedEvent args)
    {
        if (_gunSystem.GetChamberEntity(ent) is not { } ammo)
            return;

        if (!TryComp(ammo, out AmmoDelayComponent? delay))
            return;

        if (ent.Comp.Delay is { })
        {
            ent.Comp.Delay = delay.Delay;
            return;
        }
        ent.Comp.MinimumDelay = delay.MinimumDelay;
        ent.Comp.MaximumDelay = delay.MaximumDelay;
        Dirty(args.Used.Owner, ent.Comp);
    }
}
*/