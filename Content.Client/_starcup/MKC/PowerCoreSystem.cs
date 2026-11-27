using Content.Shared._starcup.MKC;
using Content.Shared.Alert;
using Content.Shared.Body;
using Content.Shared.Power.Components;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._starcup.MKC;

public sealed class PowerCoreSystem : SharedPowerCoreSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;

    private static readonly TimeSpan AlertUpdateDelay = TimeSpan.FromSeconds(0.5f);
    private TimeSpan _nextAlertUpdate = TimeSpan.Zero;
    private EntityQuery<PowerCoreComponent> _powerCoreQuery;
    private EntityQuery<BodyComponent> _bodyQuery;

    private readonly ProtoId<AlertPrototype> _batteryAlertPrototype = "PowerCore";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerCoreComponent, BodyRelayedEvent<LocalPlayerAttachedEvent>>(OnPlayerAttached);
        SubscribeLocalEvent<PowerCoreComponent, BodyRelayedEvent<LocalPlayerDetachedEvent>>(OnPlayerDetached);
        SubscribeLocalEvent<PowerCoreComponent, BodyRelayedEvent<OrganGotRemovedEvent>>(OnOrganRemoved);

        _powerCoreQuery = GetEntityQuery<PowerCoreComponent>();
        _bodyQuery = GetEntityQuery<BodyComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateBattery();
    }

    private void UpdateBattery()
    {
        if (_player.LocalEntity is not {} localPlayer)
            return;

        if (_timing.CurTime < _nextAlertUpdate)
            return;

        _nextAlertUpdate = _timing.CurTime + AlertUpdateDelay;

        if (!_bodyQuery.TryComp(localPlayer, out var body))
            return;

        if (body.Organs == null)
            return;

        foreach (var organ in body.Organs.ContainedEntities)
        {
            if (!_powerCoreQuery.TryComp(organ, out var powerCore))
                continue;

            UpdateBatteryAlert(localPlayer, (organ, powerCore));
        }
    }

    private void UpdateBatteryAlert(EntityUid body, Entity<PowerCoreComponent> powerCore)
    {
        var battery = new Entity<BatteryComponent?>(powerCore.Owner, null);
        if (!Resolve(powerCore.Owner, ref battery.Comp))
            return;

        if (!TryComp(body, out AlertsComponent? alerts))
            return;

        // alert levels from 0 to 10
        var chargeLevel = _battery.GetChargeLevel(battery);
        var alertLevel = (int) MathF.Round(chargeLevel * 10f);
        alertLevel = chargeLevel > 0 ? Math.Max(alertLevel, 1) : 0;

        _alerts.ShowAlert((body, alerts), _batteryAlertPrototype, (short) alertLevel);
    }

    private void OnPlayerAttached(Entity<PowerCoreComponent> powerCore, ref BodyRelayedEvent<LocalPlayerAttachedEvent> args)
    {
        UpdateBatteryAlert(args.Body.Owner, powerCore);
    }

    private void OnPlayerDetached(Entity<PowerCoreComponent> powerCore, ref BodyRelayedEvent<LocalPlayerDetachedEvent> args)
    {
        _alerts.ClearAlert(args.Body.Owner, _batteryAlertPrototype);
    }

    private void OnOrganRemoved(Entity<PowerCoreComponent> powerCore, ref BodyRelayedEvent<OrganGotRemovedEvent> args)
    {
        _alerts.ClearAlert(args.Body.Owner, _batteryAlertPrototype);
    }
}
