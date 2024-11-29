using Content.Server.DeltaV.Station.Components;
using Content.Server.DeltaV.Station.Events;

namespace Content.Server.DeltaV.Station.Systems;

public sealed class CaptainStateSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<CaptainStateComponent, ComponentInit> (OnComponentInit);

        base.Initialize();
    }

    private void OnComponentInit(Entity<CaptainStateComponent> ent, ref ComponentInit args)
    {
    }
}
