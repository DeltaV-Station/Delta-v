using Content.Shared.DeviceLinking.Events;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Power.Components;
using Content.Shared.DeviceLinking;
using Content.Shared._DV.DeviceLinking.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.DeviceLinking.Systems;

public sealed class PowerTogglePortSystem : EntitySystem
{
    [Dependency] private readonly SharedDeviceLinkSystem _device = default!;

    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerTogglePortComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnSignalReceived(Entity<PowerTogglePortComponent> entity, ref SignalReceivedEvent args)
    {
        if (args.Port == entity.Comp.PowerTogglePort)
            _power.TogglePower(entity);
    }

}
