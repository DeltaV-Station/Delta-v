using Content.Server.DeltaV.Station.Components;
using Content.Server.DeltaV.Station.Events;

namespace Content.Server.DeltaV.Station.Systems;

public sealed class CaptainStateSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<CaptainStateComponent, ComponentShutdown>(OnComponentShutdown);

        base.Initialize();
    }

    private void OnComponentShutdown(Entity<CaptainStateComponent> ent, ref ComponentShutdown args)
    {
    }
}
