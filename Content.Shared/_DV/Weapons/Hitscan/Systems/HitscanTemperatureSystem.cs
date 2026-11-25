using Content.Shared.Temperature.Systems;
using Content.Shared._DV.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Hitscan.Events;

namespace Content.Shared._DV.Weapons.Hitscan.Systems;

public sealed class HitscanTemperatureSystem : EntitySystem
{
    [Dependency] private readonly SharedTemperatureSystem _temp = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HitscanTemperatureComponent, HitscanRaycastFiredEvent>(OnHitscanHit);
    }

    private void OnHitscanHit(Entity<HitscanTemperatureComponent> hitscan, ref HitscanRaycastFiredEvent args)
    {
        if (args.Data.HitEntity == null)
            return;

        _temp.ChangeHeat(args.Data.HitEntity.Value, hitscan.Comp.HeatChange, false);
    }
}
