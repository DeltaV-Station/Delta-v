using Content.Server._DV.Weapons.Ranged.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Throwing;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Server._DV.Weapons.Ranged.Systems;

public sealed class FireOnLandSystem : EntitySystem
{
    [Dependency] private readonly GunSystem _gunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FireOnLandComponent, LandEvent>(FireOnLand);
    }

    private void FireOnLand(Entity<FireOnLandComponent> ent, ref LandEvent args)
    {
        if (TryComp(ent, out GunComponent? gc))
        {
            _gunSystem.AttemptShoot(ent, gc);
        }
    }
}
