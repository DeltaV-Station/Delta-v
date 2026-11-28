using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._DV.Weapons.Ranged.Systems;

public sealed class SingleUseGunSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SingleUseGunComponent, GunShotEvent>(OnShot);
        SubscribeLocalEvent<SingleUseGunComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnShot(Entity<SingleUseGunComponent> ent, ref GunShotEvent args)
    {
        ent.Comp.Broken = true;
        Dirty(ent);
    }

    private void OnShotAttempted(Entity<SingleUseGunComponent> ent, ref ShotAttemptedEvent args)
    {
        if (ent.Comp.Broken)
            args.Cancel();
    }
}
