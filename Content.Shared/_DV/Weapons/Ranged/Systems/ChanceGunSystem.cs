using Content.Shared._DV.Weapons.Ranged.Components;
using Robust.Shared.Random;

namespace Content.Shared._DV.Weapons.Ranged.Systems;

public sealed class ChanceGunSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChanceGunComponent, ComponentStartup>(OnCreation);
    }

    private void OnCreation(Entity<ChanceGunComponent> ent, ref ComponentStartup args)
    {
        TrySpawnInContainer(_random.Pick(ent.Comp.AmmoList), ent, "gun_chamber", out _);
    }
}
