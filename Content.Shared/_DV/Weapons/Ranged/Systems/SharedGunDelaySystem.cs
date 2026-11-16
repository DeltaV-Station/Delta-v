using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Interaction.Events;
using Robust.Shared.Audio.Systems;
using Content.Shared.Hands;

namespace Content.Shared._DV.Weapons.Ranged.Systems;

public abstract class SharedGunDelaySystem : EntitySystem
{
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedGunSystem GunSystem = default!;
    [Dependency] protected readonly SharedAudioSystem AudioSystem = default!;

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

    protected abstract void OnShotAttempted(Entity<GunDelayComponent> ent, ref ShotAttemptedEvent args);

    protected abstract void CheckShot();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        CheckShot();
    }

    /// <summary>
    /// Handles the resetting after the shot is fired
    /// </summary>
    private void OnShot(Entity<GunDelayComponent> ent, ref GunShotEvent args)
    {
        ent.Comp.Lit = false;
        ent.Comp.Firing = false;
        Dirty(ent);
    }
}
