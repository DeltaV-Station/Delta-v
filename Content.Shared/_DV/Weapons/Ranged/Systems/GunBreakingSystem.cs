using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._DV.Weapons.Ranged.Systems;

public sealed class GunBreakingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunBreakingComponent, GunShotEvent>(OnShot);
        SubscribeLocalEvent<GunBreakingComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnShot(Entity<GunBreakingComponent> ent, ref GunShotEvent args)
    {
        ent.Comp.Broken = true;
        Dirty(ent);
    }

    private void OnShotAttempted(Entity<GunBreakingComponent> ent, ref ShotAttemptedEvent args)
    {
        if (ent.Comp.Broken)
            args.Cancel();
    }
}
