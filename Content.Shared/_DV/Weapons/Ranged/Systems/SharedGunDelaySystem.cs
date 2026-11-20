using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Timing;
using Content.Shared.Interaction.Events;
using Robust.Shared.Audio.Systems;
using Content.Shared.Hands;

namespace Content.Shared._DV.Weapons.Ranged.Systems;

public abstract class SharedGunDelaySystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedGunSystem GunSystem = default!;
    [Dependency] protected readonly SharedAudioSystem AudioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunDelayComponent, ShotAttemptedEvent>(OnShotAttempted, after: [typeof(SingleUseGunSystem)]);
        SubscribeLocalEvent<GunDelayComponent, DroppedEvent>(OnDropped);
        SubscribeLocalEvent<GunDelayComponent, GunShotEvent>((ent, ref _) => OnShot(ent));
        SubscribeLocalEvent<GunDelayComponent, OnEmptyGunShotEvent>((ent, ref _) => OnShot(ent));
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

    protected abstract void OnShotAttempted(Entity<GunDelayComponent> ent, ref ShotAttemptedEvent args);

    protected abstract void CheckShot();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        CheckShot();
    }

    /// <summary>
    /// Handles the resetting after the gun shoots, empty or not
    /// </summary>
    private void OnShot(Entity<GunDelayComponent> ent)
    {
        ent.Comp.Lit = false;
        ent.Comp.Firing = false;
        Dirty(ent);
    }
}
