using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;


namespace Content.Shared._DV.Weapons.Ranged.Systems;

public sealed class ChanceGunSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChanceGunComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnShotAttempted(Entity<ChanceGunComponent> ent, ref ShotAttemptedEvent args)
    {
        if (ent.Comp.Broken)
        {
            args.Cancel();
            return;
        }
        ent.Comp.Broken = true;
        Dirty(ent);
    }
}
