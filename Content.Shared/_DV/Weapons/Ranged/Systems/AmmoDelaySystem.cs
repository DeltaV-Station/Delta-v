using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;


namespace Content.Shared._DV.Weapons.Ranged.Systems;

public sealed class AmmoDelaySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AmmoDelayComponent, ShotAttemptedEvent>(OnShotAttempted);
    }
    private void OnShotAttempted(Entity<AmmoDelayComponent> ent, ref ShotAttemptedEvent args)
    {
        if (!TryComp(args.Used.Owner, out GunDelayComponent? gun))
            return;

        if (ent.Comp.Delay is { })
        {
            gun.Delay = ent.Comp.Delay;
            return;
        }
        gun.MinimumDelay = ent.Comp.MinimumDelay;
        gun.MaximumDelay = ent.Comp.MaximumDelay;
        Dirty(args.Used.Owner, gun);
    }
}
