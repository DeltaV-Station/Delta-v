using Content.Shared._DV.Mind;

namespace Content.Shared._DV.Abilities.Psionics;

public abstract class SharedFracturedFormPowerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FracturedFormBodyComponent, ShowSSDIndicatorEvent>(OnShowSSDIndicator);
    }

    private void OnShowSSDIndicator(Entity<FracturedFormBodyComponent> entity, ref ShowSSDIndicatorEvent args)
    {
        if (HasComp<FracturedFormPowerComponent>(entity))
            return;
        args.Hidden = true;
    }
}
